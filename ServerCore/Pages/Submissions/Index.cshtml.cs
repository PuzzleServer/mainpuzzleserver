using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Submissions
{
    public class IndexModel : EventSpecificPageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public IndexModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

        public PuzzleStatePerTeam PuzzleState { get; set; }
        [BindProperty]
        public Submission Submission { get; set; }

        public IList<Submission> Submissions { get; set; }

        public int PuzzleId { get; set; }

        public int TeamId { get; set; }

        public string AnswerToken { get; set; }

        public async Task<IActionResult> OnPostAsync(int puzzleId, int teamId)
        {

            // Don't allow submissions after the answer has been found.
            if (AnswerToken != null)
            {
                return Page();
            }

            // TODO: Once auth exists, we need to check if the team has access
            // to this puzzle.

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await SetupContext(puzzleId, teamId);

            // Create submission and add it to list
            Submission.TimeSubmitted = DateTime.UtcNow;
            Submission.Puzzle = PuzzleState.Puzzle;
            Submission.Team = PuzzleState.Team;
            Submission.Response = await _context.Responses.Where(
                r => r.Puzzle.ID == puzzleId &&
                     Submission.SubmissionText == r.SubmittedText)
                .FirstOrDefaultAsync();

            Submissions.Add(Submission);

            // Update puzzle state if submission was correct
            if (Submission.Response != null && Submission.Response.IsSolution)
            {
                PuzzleState.IsSolved = true;
                PuzzleState.SolvedTime = Submission.TimeSubmitted;
            }
            else if (Submission.Response == null)
            {
                // If the submission was incorrect and not a partial solution,
                // we will do the lockout computations now.
                double? lockoutTime = ComputeLockoutTime(Event,
                                                         Submissions,
                                                         PuzzleState);
                if (lockoutTime != null)
                {
                    PuzzleState.SetPuzzleLockout((double)lockoutTime);
                }
                // We also determine if the puzzle should be set to email-only mode.
                if (IsPuzzleSubmissiongLimitReached(Event, Submissions))
                {
                    PuzzleState.IsEmailOnlyMode = true;
                }
            }
            
            _context.Submissions.Add(Submission);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Submissions/Index", new { puzzleid = puzzleId, teamid = teamId });
        }

        public async Task<IActionResult> OnGetAsync(int puzzleId, int teamId)
        {
            // TODO: Once auth exists, we need to check if the team has access
            // to this puzzle.

            await SetupContext(puzzleId, teamId);
            Submission correctSubmission = this.Submissions?.Where(
                (s) => s.Response != null &&
                       s.Response.IsSolution
                ).FirstOrDefault();

            if (correctSubmission != null)
            {
                AnswerToken = correctSubmission.SubmissionText;
            }

            return Page();
        }

        private async Task SetupContext(int puzzleId, int teamId)
        {
            PuzzleId = puzzleId;
            TeamId = teamId;

            Puzzle puzzle = await _context.Puzzles.Where(
                (p) => p.ID == puzzleId).FirstOrDefaultAsync();

            Team team = await _context.Teams.Where(
                (t) => t.ID == teamId).FirstOrDefaultAsync();

            PuzzleState = await (await PuzzleStateHelper.GetFullReadWriteQueryAsync(
                _context,
                Event,
                puzzle,
                team)).FirstOrDefaultAsync();

            // Note: These submissions are not guaranteed to be sorted, but
            // they should be entered into the database in-order.
            Submissions = await _context.Submissions.Where(
                (s) => s.Team != null &&
                       s.Team.ID == teamId &&
                       s.Puzzle != null &&
                       s.Puzzle.ID == puzzleId)
                .ToListAsync();
        }

        /// <summary>
        ///     Computes whether a team should be locked out from submitting to this puzzle
        ///     and returns for how long the lockout should be.
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="submissions">Expects submissions in chronological order</param>
        /// <param name="puzzleState"></param>
        /// <returns>
        ///     The number of minutes the team should be locked out of the puzzle or null
        ///     if the team should not be locked out.
        /// </returns>
        private static double? ComputeLockoutTime(
            Event ev,
            IList<Submission> submissions,
            PuzzleStatePerTeam puzzleState)
        {
            uint consecutiveWrongSubmissions = 0;
            bool shouldLockout = false;

            /**
             * Count the number of submissions in the past N minutes where N is the LockoutSpamDuration
             * set for the event. If that count exceeds the LockoutSpamCount set for the event, then
             * the team should be locked out of that puzzle.
             */

            foreach (Submission s in submissions.Reverse())
            {
                if (s.Response != null)
                {
                    break;
                }

                if (s.TimeSubmitted.AddMinutes(ev.LockoutSpamDuration).CompareTo(
                        DateTime.UtcNow) < 0)
                {
                    break;
                }

                if (++consecutiveWrongSubmissions >= ev.LockoutSpamCount)
                {
                    shouldLockout = true;
                    break;
                }
            }

            if (shouldLockout)
            {
                /**
                 * If this is the team's first lockout for the puzzle or If enough times has passed
                 * since the team's last lockout, we give them the minimum lockout duration, 1 minute.
                 */
                if (puzzleState.LockoutTime == null ||
                    (DateTime.UtcNow - puzzleState.LockoutTime)?.TotalMinutes >= ev.LockoutForgivenessTime)
                {
                    return 1.0;
                }

                return puzzleState.LockoutStage * ev.LockoutDurationMultiplier;
            }

            return null;
        }

        private static bool IsPuzzleSubmissiongLimitReached(Event ev, IList<Submission> submissions)
        {
            uint wrongSubmissions = 0;

            foreach (Submission s in submissions)
            {
                if (s.Response == null)
                {
                    wrongSubmissions++;
                }
            }

            return wrongSubmissions >= ev.MaxSubmissionCount;
        }
    }
}