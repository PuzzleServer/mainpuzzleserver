using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Submissions
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
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

            if (!this.Event.IsAnswerSubmissionActive)
            {
                return RedirectToPage("/Submissions/Index", new { puzzleid = puzzleId, teamid = teamId });
            }

            // TODO: Once auth exists, we need to check if the team has access
            // to this puzzle.

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await SetupContext(puzzleId, teamId);

            // Don't allow submissions after the answer has been found.
            if (PuzzleState.SolvedTime != null)
            {
                return Page();
            }


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
                await PuzzleStateHelper.SetSolveStateAsync(_context,
                    Event,
                    Submission.Puzzle,
                    Submission.Team,
                    Submission.TimeSubmitted);

            }
            else if (Submission.Response == null)
            {
                // We also determine if the puzzle should be set to email-only mode.
                if (IsPuzzleSubmissionLimitReached(
                        Event,
                        Submissions,
                        PuzzleState))
                {
                    await PuzzleStateHelper.SetEmailOnlyModeAsync(_context,
                        Event,
                        Submission.Puzzle,
                        Submission.Team,
                        true);
                }
                else
                {
                    // If the submission was incorrect and not a partial solution,
                    // we will do the lockout computations now.
                    DateTime? lockoutExpiryTime = ComputeLockoutExpiryTime(
                        Event,
                        Submissions,
                        PuzzleState);

                    if (lockoutExpiryTime != null)
                    {
                        await PuzzleStateHelper.SetLockoutExpiryTimeAsync(_context,
                            Event,
                            Submission.Puzzle,
                            Submission.Team,
                            lockoutExpiryTime);

                    }
                }
            }
            
            _context.Submissions.Add(Submission);
            await _context.SaveChangesAsync();

            return RedirectToPage(
                "/Submissions/Index",
                new { puzzleid = puzzleId, teamid = teamId });

        }

        public async Task<IActionResult> OnGetAsync(int puzzleId, int teamId)
        {
            // TODO: Once auth exists, we need to check if the team has access
            // to this puzzle.

            await SetupContext(puzzleId, teamId);
            
            if (PuzzleState.SolvedTime != null)
            {
                Submission correctSubmission = this.Submissions?.Last();
                if (correctSubmission != null)
                {
                    AnswerToken = correctSubmission.SubmissionText;
                }
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

            PuzzleState = await (PuzzleStateHelper
                .GetFullReadOnlyQuery(
                    _context,
                    Event,
                    puzzle,
                    team))
                .FirstOrDefaultAsync();

            Submissions = await _context.Submissions.Where(
                (s) => s.Team != null &&
                       s.Team.ID == teamId &&
                       s.Puzzle != null &&
                       s.Puzzle.ID == puzzleId)
                .OrderBy(submission => submission.TimeSubmitted)
                .ToListAsync();
        }

        /// <summary>
        ///     Determines if the team should be locked out for their most
        ///     recent incorrect submission and returns the expiry time for the
        ///     lockout.
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="submissions">
        ///     Expects submissions in chronological order
        /// </param>
        /// <param name="puzzleState"></param>
        /// <returns>
        ///     If the team should be locked out, returns the time when a team
        ///     can enter submissions again.
        ///     Null if the team should not be locked out.
        /// </returns>
        private static DateTime? ComputeLockoutExpiryTime(
            Event ev,
            IList<Submission> submissions,
            PuzzleStatePerTeam puzzleState)
        {
            int consecutiveWrongSubmissions = 0;

            /**
             * Count the number of submissions in the past N minutes where N is
             * the LockoutIncorrectGuessPeriod set for the event. If that count
             * exceeds the LockoutIncorrectGuessLimit set for the event, then
             * the team should be locked out of that puzzle.
             */

            foreach (Submission s in submissions.Reverse())
            {
                // Do not increment on partials
                if (s.Response != null)
                {
                    continue;
                }

                if (s.TimeSubmitted.AddMinutes(ev.LockoutIncorrectGuessPeriod)
                        .CompareTo(DateTime.UtcNow) < 0)
                {
                    break;
                }

                ++consecutiveWrongSubmissions;
            }

            if (consecutiveWrongSubmissions <= ev.LockoutIncorrectGuessLimit) {
                return null;
            }

            /**
             * The lockout duration is determined by the difference between the
             * count of wrong submissions in the lockout period and the lockout
             * limit. That difference is multiplied by the event's 
             * LockoutDurationMultiplier to determine the lockout time in
             * minutes.
             */

            return DateTime.UtcNow.AddMinutes(
                (consecutiveWrongSubmissions -
                 ev.LockoutIncorrectGuessLimit) *
                ev.LockoutDurationMultiplier);

        }

        private static bool IsPuzzleSubmissionLimitReached(
            Event ev,
            IList<Submission> submissions,
            PuzzleStatePerTeam puzzleState)
        {
            uint wrongSubmissions = 0;

            foreach (Submission s in submissions)
            {
                if (s.Response == null)
                {
                    wrongSubmissions++;
                }
            }

            if (wrongSubmissions < puzzleState.WrongSubmissionCountBuffer)
            {
                return false;
            }

            return wrongSubmissions - puzzleState.WrongSubmissionCountBuffer >=
                ev.MaxSubmissionCount;

        }
    }
}