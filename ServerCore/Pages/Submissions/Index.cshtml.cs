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

        public string SubmissionText { get; set; }

        private IList<Submission> Submissions { get; set; }

        public List<SubmissionView> SubmissionViews { get; set; }

        public Puzzle Puzzle { get; set; }

        public Team Team { get; set; }

        public string AnswerToken { get; set; }

        public IList<Puzzle> PuzzlesCausingGlobalLockout { get; set; }

        public bool DuplicateSubmission { get; set; }

        public string AnswerRedAlertMessage { get; set; }

        public string AnswerYellowAlertMessage { get; set; }

        public class SubmissionView
        {
            public Submission Submission { get; set; }
            public Response Response { get; set; }
            public string SubmitterName { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(int puzzleId, string submissionText)
        {
            if (String.IsNullOrWhiteSpace(submissionText))
            {
                ModelState.AddModelError("submissionText", "Your answer cannot be empty");
            }

            SubmissionText = submissionText;
            if (DateTime.UtcNow < Event.EventBegin)
            {
                return NotFound("The event hasn't started yet!");
            }

            await SetupContext(puzzleId);

            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            // Don't allow submissions if the team is locked out.
            if (PuzzleState.IsTeamLockedOut)
            {
                return Page();
            }

            // Don't allow submissions if team is in email only mode.
            if (PuzzleState.IsEmailOnlyMode)
            {
                return Page();
            }

            // Don't allow submissions after the answer has been found.
            if (PuzzleState.SolvedTime != null)
            {
                return Page();
            }

            // Don't accept posted submissions when a puzzle is causing lockout
            if (PuzzlesCausingGlobalLockout.Count != 0 && !PuzzlesCausingGlobalLockout.Contains(Puzzle))
            {
                return Page();
            }

            // Soft enforcement of duplicates to give a friendly message in most cases
            DuplicateSubmission = (from sub in Submissions
                                   where sub.SubmissionText == ServerCore.DataModel.Response.FormatSubmission(submissionText)
                                   select sub).Any();

            if (DuplicateSubmission)
            {
                return Page();
            }

            // Create submission and add it to list
            Submission submission = new Submission
            {
                SubmissionText = submissionText,
                TimeSubmitted = DateTime.UtcNow,
                Puzzle = PuzzleState.Puzzle,
                Team = PuzzleState.Team,
                Submitter = LoggedInUser
            };
            submission.Response = await _context.Responses.Where(
                r => r.Puzzle.ID == puzzleId &&
                     submission.SubmissionText == r.SubmittedText)
                .FirstOrDefaultAsync();

            Submissions.Add(submission);

            AnswerRedAlertMessage = null;
            // Update puzzle state if submission was correct
            if (submission.Response != null)
            {
                if (submission.Response.IsSolution)
                {
                    await PuzzleStateHelper.SetSolveStateAsync(_context,
                        Event,
                        submission.Puzzle,
                        submission.Team,
                        submission.TimeSubmitted);

                    AnswerToken = submission.SubmissionText;
                }
                else
                {
                    // If partial, put it as a warning text
                    AnswerYellowAlertMessage = string.Format("Partial Answer: {0}", submission.Response.ResponseText);
                }
            }
            else if (submission.Response == null && Event.IsAnswerSubmissionActive)
            {
                AnswerRedAlertMessage = "Incorrect";

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

                    var authors = await _context.PuzzleAuthors.Where((pa) => pa.Puzzle == submission.Puzzle).Select((pa) => pa.Author.Email).ToListAsync();

                    MailHelper.Singleton.SendPlaintextBcc(authors,
                        $"{Event.Name}: Team {submission.Team.Name} is in email mode for {submission.Puzzle.Name}",
                        "");
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

            SubmissionViews.Add(new SubmissionView()
            {
                Submission = submission,
                Response = submission.Response,
                SubmitterName = LoggedInUser.Name
            });

            return Page();
        }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            await SetupContext(puzzleId);

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
                .FirstAsync();

            SubmissionViews = await (from submission in _context.Submissions
                                     join user in _context.PuzzleUsers on submission.Submitter equals user
                                     join r in _context.Responses on submission.Response equals r into responses
                                     from response in responses.DefaultIfEmpty()
                                     where submission.Team == Team &&
                                     submission.Puzzle == Puzzle
                                     orderby submission.TimeSubmitted
                                     select new SubmissionView()
                                     {
                                         Submission = submission,
                                         Response = response,
                                         SubmitterName = user.Name,
                                     }).ToListAsync();

            Submissions = new List<Submission>(SubmissionViews.Count);
            foreach (SubmissionView submissionView in SubmissionViews)
            {
                Submissions.Add(submissionView.Submission);
            }

            PuzzlesCausingGlobalLockout = await PuzzleStateHelper.PuzzlesCausingGlobalLockout(_context, Event, Team).ToListAsync();

            if (PuzzleState.SolvedTime != null)
            {
                if (Submissions?.Count > 0)
                {
                    AnswerToken = Submissions.Last().SubmissionText;
                }
                else
                {
                    AnswerToken = "(marked as solved by admin or author)";
                }
            }
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
            DateTime incorrectGuessStartTime = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(ev.LockoutIncorrectGuessPeriod));

            foreach (Submission s in submissions)
            {
                // if the guess is before the incorrect window, ignore it
                if (s.TimeSubmitted < incorrectGuessStartTime)
                {
                    continue;
                }

                if (s.Response == null)
                {
                    ++consecutiveWrongSubmissions;
                }
                else
                {
                    // Do not increment on partials, decrement instead! This is a tweak to improve the lockout story for DC puzzles.
                    // But don't overdecrement, lest we let people build a gigantic bank of free guesses.
                    consecutiveWrongSubmissions = Math.Max(0, consecutiveWrongSubmissions - 1);
                }
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
