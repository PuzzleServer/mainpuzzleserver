using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Helpers
{

    public enum SubmissionResponseCode
    {
        Correct,
        Incorrect,
        Freeform,
        Partial,
        Unauthorized,
        PuzzleNotFound,
        PuzzleLocked,
        EmptySubmission,
        TeamLockedOut,
        AlreadySolved,
        DuplicateSubmission
    }

    public class SubmissionResponse
    {
        public SubmissionResponseCode ResponseCode { get; set; }
        public Response CompleteResponse { get; set; }
        public string FreeformResponse { get; set; }
    }

    public class SubmissionView
    {
        public Submission Submission { get; set; }
        public Response Response { get; set; }
        public string SubmitterName { get; set; }
        public bool IsFreeform { get; set; }
        public string FreeformReponse { get; set; }
    }

    public class SubmissionEvaluator
    {
        private static PuzzleStatePerTeam puzzleState;

        private static Puzzle puzzle;

        private static Team team;

        private static Event thisEvent;

        private static IList<Puzzle> puzzlesCausingGlobalLockout;

        private static bool duplicateSubmission;

        private static IList<Submission> submissions;

        public static List<SubmissionView> submissionViews;

        public static PuzzleUser loggedInUser;


        public static async Task<SubmissionResponse> EvaluateSubmission(PuzzleServerContext context, PuzzleUser loggedInUser, Event anEvent, int puzzleId, string submissionText, bool allowFreeformSharing)
        {
            thisEvent = anEvent;

            team = await UserEventHelper.GetTeamForPlayer(context, thisEvent, loggedInUser);

            puzzle = await context.Puzzles.Where(
                (p) => p.ID == puzzleId).FirstOrDefaultAsync();

            puzzleState = await (PuzzleStateHelper
                .GetFullReadOnlyQuery(
                    context,
                    thisEvent,
                    puzzle,
                    team))
                .FirstAsync();

            submissionViews = await (from sub in context.Submissions
                                     join user in context.PuzzleUsers on sub.Submitter equals user
                                     join r in context.Responses on sub.Response equals r into responses
                                     from response in responses.DefaultIfEmpty()
                                     where sub.Team == team &&
                                     sub.Puzzle == puzzle
                                     orderby sub.TimeSubmitted
                                     select new SubmissionView()
                                     {
                                         Submission = sub,
                                         Response = response,
                                         SubmitterName = user.Name,
                                         FreeformReponse = sub.FreeformResponse,
                                         IsFreeform = puzzle.IsFreeform
                                     }).ToListAsync();

            submissions = new List<Submission>(submissionViews.Count);
            foreach (SubmissionView submissionView in submissionViews)
            {
                submissions.Add(submissionView.Submission);
            }

            puzzlesCausingGlobalLockout = await PuzzleStateHelper.PuzzlesCausingGlobalLockout(context, thisEvent, team).ToListAsync();

            if (String.IsNullOrWhiteSpace(submissionText))
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.EmptySubmission };
            }

            team = await UserEventHelper.GetTeamForPlayer(context, thisEvent, loggedInUser);

            if (loggedInUser == null || team == null)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Unauthorized };
            }

            puzzle = await context.Puzzles.Where(
                (p) => p.ID == puzzleId).FirstOrDefaultAsync();

            if (puzzle == null || puzzleState.UnlockedTime == null)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.PuzzleLocked };
            }

            if (DateTime.UtcNow < thisEvent.EventBegin)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Unauthorized };
            }

            puzzleState = await (PuzzleStateHelper
                          .GetFullReadOnlyQuery(
                              context,
                              thisEvent,
                              puzzle,
                              team))
                          .FirstAsync();

            if (puzzleState.IsTeamLockedOut || puzzleState.IsEmailOnlyMode)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.TeamLockedOut };
            }

            if (puzzleState.SolvedTime != null)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.AlreadySolved };
            }

            puzzlesCausingGlobalLockout = await PuzzleStateHelper.PuzzlesCausingGlobalLockout(context, thisEvent, team).ToListAsync();

            if (puzzlesCausingGlobalLockout.Count != 0 && !puzzlesCausingGlobalLockout.Contains(puzzle))
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.TeamLockedOut };
            }

            duplicateSubmission = (from sub in submissions
                                   where sub.SubmissionText == Response.FormatSubmission(submissionText)
                                   select sub).Any();

            if (duplicateSubmission)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.DuplicateSubmission };
            }

            // Create submission and add it to list
            Submission submission = new Submission
            {
                TimeSubmitted = DateTime.UtcNow,
                Puzzle = puzzleState.Puzzle,
                Team = puzzleState.Team,
                Submitter = loggedInUser,
                AllowFreeformSharing = allowFreeformSharing
            };

            string submissionTextToCheck = Response.FormatSubmission(submissionText);

            if (puzzle.IsFreeform)
            {
                submission.UnformattedSubmissionText = submissionText;
            }
            else
            {
                submission.SubmissionText = submissionText;
            }

            submission.Response = await context.Responses.Where(
                r => r.Puzzle.ID == puzzleId &&
                     submissionTextToCheck == r.SubmittedText)
                .FirstOrDefaultAsync();

            submissions.Add(submission);

            // Update puzzle state if submission was correct
            if (submission.Response != null && submission.Response.IsSolution)
            {
                await PuzzleStateHelper.SetSolveStateAsync(context,
                    thisEvent,
                    submission.Puzzle,
                    submission.Team,
                    submission.TimeSubmitted);
            }
            else if (!puzzle.IsFreeform && submission.Response == null && thisEvent.IsAnswerSubmissionActive)
            {
                // We also determine if the puzzle should be set to email-only mode.
                if (IsPuzzleSubmissionLimitReached(
                        thisEvent,
                        submissions,
                        puzzleState))
                {
                    await PuzzleStateHelper.SetEmailOnlyModeAsync(context,
                        thisEvent,
                        submission.Puzzle,
                        submission.Team,
                        true);

                    var authors = await context.PuzzleAuthors.Where((pa) => pa.Puzzle == submission.Puzzle).Select((pa) => pa.Author.Email).ToListAsync();

                    MailHelper.Singleton.SendPlaintextBcc(authors,
                        $"{thisEvent.Name}: Team {submission.Team.Name} is in email mode for {submission.Puzzle.Name}",
                        "");
                }
                else
                {
                    // If the submission was incorrect and not a partial solution,
                    // we will do the lockout computations now.
                    DateTime? lockoutExpiryTime = ComputeLockoutExpiryTime(
                        thisEvent,
                        submissions,
                        puzzleState);

                    if (lockoutExpiryTime != null)
                    {
                        await PuzzleStateHelper.SetLockoutExpiryTimeAsync(context,
                            thisEvent,
                            submission.Puzzle,
                            submission.Team,
                            lockoutExpiryTime);
                    }
                }
            }

            context.Submissions.Add(submission);
            await context.SaveChangesAsync();

            if (submission.Response != null && submission.Response.IsSolution)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Correct };
            }

            if (submission.Response != null && !submission.Response.IsSolution)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Partial, CompleteResponse = submission.Response };
            }

            if (!puzzle.IsFreeform && submission.Response == null && thisEvent.IsAnswerSubmissionActive)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Freeform, FreeformResponse = submission.FreeformResponse };
            }

            return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Unauthorized };
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

            if (consecutiveWrongSubmissions <= ev.LockoutIncorrectGuessLimit)
            {
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
