using System;
using System.Collections.Generic;
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
        public const string IncorrectResponseText = "Incorrect";

        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }
        public bool IsPuzzleForSinglePlayer { get; set; }

        public PuzzleStateBase PuzzleState { get; set; }

        public string SubmissionText { get; set; }

        private IList<SubmissionBase> Submissions { get; set; }

        public List<SubmissionView> SubmissionViews { get; set; }

        public Puzzle Puzzle { get; set; }

        public string PuzzleAuthor { get; set; }

        public Team Team { get; set; }

        public string AnswerToken { get; set; }

        public IList<Puzzle> PuzzlesCausingGlobalLockout { get; set; }

        public string AnswerRedAlertMessage { get; set; }

        public string AnswerYellowAlertMessage { get; set; }

        [BindProperty]
        public bool AllowFreeformSharing { get; set; }

        public class SubmissionView
        {
            public SubmissionBase Submission { get; set; }
            public Response Response { get; set; }
            public string SubmitterName { get; set; }
            public bool IsFreeform { get; set; }
            public string FreeformReponse { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(int puzzleId, string submissionText)
        {
            AnswerRedAlertMessage = null;
            AnswerYellowAlertMessage = null;
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
            if (PuzzleState.IsLockedOut)
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
            SubmissionBase duplicatedSubmission = (from sub in Submissions
                                   where sub.SubmissionText == ServerCore.DataModel.Response.FormatSubmission(submissionText)
                                   select sub).FirstOrDefault();

            if (duplicatedSubmission != null)
            {
                AnswerRedAlertMessage = $"Oops! You've already submitted \"{submissionText}\"";
                if (!Puzzle.IsFreeform)
                {
                    string response = duplicatedSubmission.Response?.ResponseText ?? IndexModel.IncorrectResponseText;
                    AnswerRedAlertMessage += $", which had the response \"{response}\"";
                }

                return Page();
            }

            // Create submission and add it to list
            SubmissionBase submission = IsPuzzleForSinglePlayer
                ? new SinglePlayerPuzzleSubmission
                {
                    TimeSubmitted = DateTime.UtcNow,
                    Puzzle = PuzzleState.Puzzle,
                    Submitter = LoggedInUser,
                    AllowFreeformSharing = AllowFreeformSharing
                }
                : new Submission
                {
                    TimeSubmitted = DateTime.UtcNow,
                    Puzzle = PuzzleState.Puzzle,
                    Team = Team,
                    Submitter = LoggedInUser,
                    AllowFreeformSharing = AllowFreeformSharing
                };

            string submissionTextToCheck = ServerCore.DataModel.Response.FormatSubmission(submissionText);
            if (Puzzle.IsFreeform)
            {
                submission.UnformattedSubmissionText = submissionText;
            }
            else
            {
                submission.SubmissionText = submissionText;
            }

            submission.Response = await _context.Responses.Where(
                r => r.Puzzle.ID == puzzleId &&
                     submissionTextToCheck == r.SubmittedText)
                .FirstOrDefaultAsync();

            Submissions.Add(submission);

            if (submission.Response != null)
            {
                if (submission.Response.IsSolution)
                {
                    // Update puzzle state if submission was correct
                    if (IsPuzzleForSinglePlayer)
                    {
                        await SinglePlayerPuzzleStateHelper.SetSolveStateAsync(_context,
                            Event,
                            submission.Puzzle.ID,
                            LoggedInUser.ID,
                            submission.TimeSubmitted);
                    }
                    else
                    {
                        await PuzzleStateHelper.SetSolveStateAsync(_context,
                            Event,
                            submission.Puzzle,
                            Team,
                            submission.TimeSubmitted);
                    }

                    AnswerToken = submission.SubmissionText;
                }
                else
                {
                    // Is partial
                    AnswerYellowAlertMessage = string.Format($"\"{submission.SubmissionText}\" has partial answer: \"{submission.Response.ResponseText}\"");
                }
            }
            else if (!Puzzle.IsFreeform && submission.Response == null && Event.IsAnswerSubmissionActive)
            {
                AnswerRedAlertMessage = $"\"{submission.SubmissionText}\" is incorrect";

                // We also determine if the puzzle should be set to email-only mode.
                if (IsPuzzleSubmissionLimitReached(
                        Event,
                        Submissions,
                        PuzzleState))
                {
                    if (IsPuzzleForSinglePlayer)
                    {
                        await SinglePlayerPuzzleStateHelper.SetEmailOnlyModeAsync(_context,
                            Event,
                            submission.Puzzle.ID,
                            submission.Submitter.ID,
                            true);
                    }
                    else
                    {
                        await PuzzleStateHelper.SetEmailOnlyModeAsync(_context,
                            Event,
                            submission.Puzzle,
                            Team,
                            true);
                    }

                    var authors = await _context.PuzzleAuthors.Where((pa) => pa.Puzzle == submission.Puzzle).Select((pa) => pa.Author.Email).ToListAsync();
                    string affectedEntity = IsPuzzleForSinglePlayer ? $"User {LoggedInUser.Name ?? LoggedInUser.Email}" : $"Team {Team.Name}";
                    MailHelper.Singleton.SendPlaintextBcc(authors,
                        $"{Event.Name}: {affectedEntity} is in email mode for {submission.Puzzle.Name}",
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
                        if (IsPuzzleForSinglePlayer)
                        {
                            await SinglePlayerPuzzleStateHelper.SetLockoutExpiryTimeAsync(_context,
                                Event,
                                submission.Puzzle.ID,
                                LoggedInUser.ID,
                                lockoutExpiryTime);
                        }
                        else
                        {
                            await PuzzleStateHelper.SetLockoutExpiryTimeAsync(_context,
                                Event,
                                submission.Puzzle,
                                Team,
                                lockoutExpiryTime);
                        }
                    }
                }
            }

            Submission teamSubmission = submission as Submission;
            SinglePlayerPuzzleSubmission playerSubmission = submission as SinglePlayerPuzzleSubmission;
            if (teamSubmission != null)
            {
                _context.Submissions.Add(teamSubmission);
            }
            else if (playerSubmission != null)
            {
                _context.SinglePlayerPuzzleSubmissions.Add(playerSubmission);
            }

            await _context.SaveChangesAsync();

            SubmissionViews.Add(new SubmissionView()
            {
                Submission = submission,
                Response = submission.Response,
                SubmitterName = LoggedInUser.Name,
                IsFreeform = Puzzle.IsFreeform
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

            IsPuzzleForSinglePlayer = Puzzle.IsForSinglePlayer;
            List<SubmissionView> submissionViews = new List<SubmissionView>();
            if (Puzzle.IsForSinglePlayer)
            {
                PuzzleState = await SinglePlayerPuzzleStateHelper.GetOrAddStateIfNotThere(_context,
                    Event,
                    Puzzle,
                    LoggedInUser.ID);

                SubmissionViews = await (from submission in _context.SinglePlayerPuzzleSubmissions
                                         where submission.Puzzle == Puzzle
                                            && submission.Submitter.ID == LoggedInUser.ID
                                         join r in _context.Responses on submission.Response equals r into responses
                                         from response in responses.DefaultIfEmpty()
                                         orderby submission.TimeSubmitted
                                         select new SubmissionView()
                                         {
                                             Submission = submission,
                                             Response = response,
                                             SubmitterName = LoggedInUser.Name,
                                             FreeformReponse = submission.FreeformResponse,
                                             IsFreeform = Puzzle.IsFreeform
                                         }).ToListAsync();

                PuzzlesCausingGlobalLockout = await SinglePlayerPuzzleStateHelper.PuzzlesCausingGlobalLockout(_context, Event, LoggedInUser.ID).ToListAsync();
            }
            else
            {
                Team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);

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
                                             FreeformReponse = submission.FreeformResponse,
                                             IsFreeform = Puzzle.IsFreeform
                                         }).ToListAsync();

                PuzzlesCausingGlobalLockout = await PuzzleStateHelper.PuzzlesCausingGlobalLockout(_context, Event, Team).ToListAsync();
            }

            IQueryable<PuzzleUser> currentAuthorsQ = _context.PuzzleAuthors.Where(m => m.Puzzle == Puzzle).Select(m => m.Author);
            List<PuzzleUser> CurrentAuthors = await currentAuthorsQ.OrderBy(p => p.Name).ToListAsync();
            PuzzleAuthor = CurrentAuthors[0].Name;
            for (int i = 1; i < CurrentAuthors.Count; i++) {
                PuzzleAuthor += ", ";
                PuzzleAuthor += CurrentAuthors[i].Name;
            }

            Submissions = new List<SubmissionBase>(SubmissionViews.Count);

            foreach (SubmissionView submissionView in SubmissionViews)
            {
                Submissions.Add(submissionView.Submission);
            }

            if (PuzzleState.SolvedTime != null)
            {
                if (!Puzzle.IsFreeform && Submissions?.Count > 0)
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
            IList<SubmissionBase> submissions,
            PuzzleStateBase puzzleState)
        {
            int consecutiveWrongSubmissions = 0;

            /**
             * Count the number of submissions in the past N minutes where N is
             * the LockoutIncorrectGuessPeriod set for the event. If that count
             * exceeds the LockoutIncorrectGuessLimit set for the event, then
             * the team should be locked out of that puzzle.
             */
            DateTime incorrectGuessStartTime = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(ev.LockoutIncorrectGuessPeriod));

            foreach (SubmissionBase s in submissions)
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
            IList<SubmissionBase> submissions,
            PuzzleStateBase puzzleState)
        {
            uint wrongSubmissions = 0;

            foreach (SubmissionBase s in submissions)
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
