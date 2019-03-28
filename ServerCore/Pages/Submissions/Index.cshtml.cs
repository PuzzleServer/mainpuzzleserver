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
                int WrongSubmissionCount = Submissions
                    .Where(s => s.Response == null)
                    .Count();

                // We also determine if the puzzle should be set to email-only mode.
                if (IsPuzzleSubmissionLimitReached(
                        Event,
                        WrongSubmissionCount,
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
                        WrongSubmissionCount);

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
        /// <param name="WrongSubmissionCount"></param>
        /// <returns>
        ///     If the team should be locked out, returns the time when a team
        ///     can enter submissions again.
        ///     Null if the team should not be locked out.
        /// </returns>
        private static DateTime? ComputeLockoutExpiryTime(
            Event ev,
            int WrongSubmissionCount)
        {
            if (WrongSubmissionCount < ev.LockoutIncorrectGuessLimit) {
                return null;
            }

            return DateTime.UtcNow.AddMinutes(Math.Max(
                ev.LockoutBaseDuration,
                WrongSubmissionCount * ev.LockoutDurationMultiplier));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="WrongSubmissionCount"></param>
        /// <param name="puzzleState"></param>
        /// <returns>
        ///     True if the team should be locked out of this puzzle.
        ///     False otherwise.
        /// </returns>
        private static bool IsPuzzleSubmissionLimitReached(
            Event ev,
            int WrongSubmissionCount,
            PuzzleStatePerTeam puzzleState)
        {
            if (WrongSubmissionCount < puzzleState.WrongSubmissionCountBuffer)
            {
                return false;
            }

            return WrongSubmissionCount - puzzleState.WrongSubmissionCountBuffer >=
                ev.MaxSubmissionCount;
        }
    }
}