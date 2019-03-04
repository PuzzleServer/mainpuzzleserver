using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Submissions
{
    [Authorize(Policy = "PlayerCanSeePuzzle")]
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public PuzzleStatePerTeam PuzzleState { get; set; }

        [BindProperty]
        [Required]
        public string SubmissionText { get; set; }

        public IList<Submission> Submissions { get; set; }

        public Puzzle Puzzle { get; set; }

        public Team Team { get; set; }

        public string AnswerToken { get; set; }

        public IList<Puzzle> PuzzlesCausingGlobalLockout { get; set; }

        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            if (!this.Event.IsAnswerSubmissionActive)
            {
                return RedirectToPage("/Submissions/Index", new { puzzleid = puzzleId });
            }

            await SetupContext(puzzleId);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Don't allow submissions after the answer has been found.
            if (PuzzleState.SolvedTime != null)
            {
                return Page();
            }

            // Create submission and add it to list
            Submission submission = new Submission
            {
                SubmissionText = SubmissionText,
                TimeSubmitted = DateTime.UtcNow,
                Puzzle = PuzzleState.Puzzle,
                Team = PuzzleState.Team,
                Submitter = LoggedInUser,
            };
            submission.Response = await _context.Responses.Where(
                r => r.Puzzle.ID == puzzleId &&
                     submission.SubmissionText == r.SubmittedText)
                .FirstOrDefaultAsync();

            Submissions.Add(submission);

            // Update puzzle state if submission was correct
            if (submission.Response != null && submission.Response.IsSolution)
            {
                await PuzzleStateHelper.SetSolveStateAsync(_context,
                    Event,
                    submission.Puzzle,
                    submission.Team,
                    submission.TimeSubmitted);

            }
            else if (submission.Response == null)
            {
                // We also determine if the puzzle should be set to email-only mode.
                if (IsPuzzleSubmissionLimitReached(
                        Event,
                        Submissions,
                        PuzzleState))
                {
                    await PuzzleStateHelper.SetEmailOnlyModeAsync(_context,
                        Event,
                        submission.Puzzle,
                        submission.Team,
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
                            submission.Puzzle,
                            submission.Team,
                            lockoutExpiryTime);

                    }
                }
            }
            
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return RedirectToPage(
                "/Submissions/Index",
                new { puzzleid = puzzleId });
        }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            // TODO: Once auth exists, we need to check if the team has access
            // to this puzzle.

            await SetupContext(puzzleId);

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

        private async Task SetupContext(int puzzleId)
        {
            Team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);

            Puzzle = await _context.Puzzles.Where(
                (p) => p.ID == puzzleId).FirstOrDefaultAsync();

            PuzzleState = await (PuzzleStateHelper
                .GetFullReadOnlyQuery(
                    _context,
                    Event,
                    Puzzle,
                    Team))
                .FirstOrDefaultAsync();

            Submissions = await _context.Submissions.Where(
                (s) => s.Team == Team &&
                       s.Puzzle == Puzzle)
                .OrderBy(submission => submission.TimeSubmitted)
                .ToListAsync();

            PuzzlesCausingGlobalLockout = await PuzzleStateHelper.PuzzlesCausingGlobalLockout(_context, Event, Team).ToListAsync();
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