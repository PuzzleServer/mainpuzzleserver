using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Helpers
{
    /// <summary>
    /// Possible responses to player submissions
    /// </summary>
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

    /// <summary>
    /// All of the components needed to process a submission response
    /// </summary>
    public class SubmissionResponse
    {
        public SubmissionResponseCode ResponseCode { get; set; }
        public string CompleteResponse { get; set; }
        public string FreeformResponse { get; set; }
    }

    /// <summary>
    /// All of the components needed to represent a player submission
    /// </summary>
    public class SubmissionView
    {
        public SubmissionBase Submission { get; set; }
        public Response Response { get; set; }
        public string SubmitterName { get; set; }
        public bool IsFreeform { get; set; }
        public string FreeformResponse { get; set; }
    }

    /// <summary>
    /// Handles player submissions
    /// </summary>
    public class SubmissionEvaluator
    {
        /// <summary>
        /// Evaulates player submissions then either saves them to the database or returns an error to the caller
        /// </summary>
        public static async Task<SubmissionResponse> EvaluateSubmission(PuzzleServerContext context, PuzzleUser loggedInUser, Event thisEvent, int puzzleId, string submissionText, bool allowFreeformSharing)
        {
            //Query data needed to process submission
            Puzzle puzzle = await context.Puzzles.Where(
                (p) => p.ID == puzzleId).FirstOrDefaultAsync();

            Team team = null;
            if (!puzzle.IsForSinglePlayer) {
                team = await UserEventHelper.GetTeamForPlayer(context, thisEvent, loggedInUser);
            }

            PuzzleStateBase puzzleState = null;
            if (!puzzle.IsForSinglePlayer)
            {
                puzzleState = await (PuzzleStateHelper
                .GetFullReadOnlyQuery(
                    context,
                    thisEvent,
                    puzzle,
                    team))
                .FirstAsync();
            }
            else 
            {
                puzzleState = await (SinglePlayerPuzzleStateHelper
                .GetFullReadOnlyQuery(
                    context,
                    thisEvent,
                    puzzle.ID,
                    loggedInUser.ID))
                .FirstAsync();
            }

            List<Puzzle> puzzlesCausingGlobalLockout = null;
            if (!puzzle.IsForSinglePlayer)
            {
                puzzlesCausingGlobalLockout = await PuzzleStateHelper.PuzzlesCausingGlobalLockout(context, thisEvent, team).ToListAsync();
            }
            else 
            {
                puzzlesCausingGlobalLockout = await SinglePlayerPuzzleStateHelper.PuzzlesCausingGlobalLockout(context, thisEvent, loggedInUser.ID).ToListAsync();
            }

            // Return early for cases when there's obviously nothing we can do with the submission
            // The submission text is empty
            if (String.IsNullOrWhiteSpace(submissionText))
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.EmptySubmission };
            }

            // The puzzle is locked
            if (puzzle == null || puzzleState.UnlockedTime == null)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.PuzzleLocked };
            }

            // The user or team isn't known
            if (loggedInUser == null || (!puzzle.IsForSinglePlayer && team == null))
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Unauthorized };
            }

            // The event hasn't started yet
            if (DateTime.UtcNow < thisEvent.EventBegin && team?.IsDisqualified != true)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Unauthorized };
            }

            // The user or team is locked out
            if (puzzleState.IsLockedOut || puzzleState.IsEmailOnlyMode)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.TeamLockedOut };
            }

            // The puzzle has already been solved
            if (puzzleState.SolvedTime != null)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.AlreadySolved };
            }

            // The user or team is under a global lockout
            if (puzzlesCausingGlobalLockout.Count != 0 && !puzzlesCausingGlobalLockout.Contains(puzzle))
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.TeamLockedOut };
            }

            List<SubmissionView> submissionViews = null;
            if (!puzzle.IsForSinglePlayer)
            {
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
                                             FreeformResponse = sub.FreeformResponse,
                                             IsFreeform = puzzle.IsFreeform
                                         }).ToListAsync();
            }
            else
            {
                submissionViews = await (from sub in context.SinglePlayerPuzzleSubmissions
                                         where sub.Puzzle == puzzle
                                            && sub.Submitter.ID == loggedInUser.ID
                                         join r in context.Responses on sub.Response equals r into responses
                                         from response in responses.DefaultIfEmpty()
                                         orderby sub.TimeSubmitted
                                         select new SubmissionView()
                                         {
                                             Submission = sub,
                                             Response = response,
                                             SubmitterName = loggedInUser.Name,
                                             FreeformResponse = sub.FreeformResponse,
                                             IsFreeform = puzzle.IsFreeform
                                         }).ToListAsync();
            }

            List<SubmissionBase> submissions = new List<SubmissionBase>(submissionViews.Count);
            foreach (SubmissionView submissionView in submissionViews)
            {
                submissions.Add(submissionView.Submission);
            }

            // The submission is a duplicate
            bool duplicateSubmission = (from sub in submissions
                                        where sub.SubmissionText == Response.FormatSubmission(submissionText)
                                        select sub).Any();

            if (duplicateSubmission)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.DuplicateSubmission };
            }

            // Create submission and add it to list
            SubmissionBase submission = (!puzzle.IsForSinglePlayer)
                ? new Submission
                {
                    TimeSubmitted = DateTime.UtcNow,
                    Puzzle = puzzleState.Puzzle,
                    Team = team,
                    Submitter = loggedInUser,
                    AllowFreeformSharing = allowFreeformSharing
                }
                : new SinglePlayerPuzzleSubmission
                {
                    TimeSubmitted = DateTime.UtcNow,
                    Puzzle = puzzleState.Puzzle,
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
                if (!puzzle.IsForSinglePlayer)
                {
                    await PuzzleStateHelper.SetSolveStateAsync(context,
                        thisEvent,
                        submission.Puzzle,
                        team,
                        submission.TimeSubmitted);
                }
                else
                {
                    await SinglePlayerPuzzleStateHelper.SetSolveStateAsync(context,
                        thisEvent,
                        submission.Puzzle,
                        loggedInUser.ID,
                        submission.TimeSubmitted);
                }
            }
            else if (!puzzle.IsFreeform && submission.Response == null && thisEvent.IsAnswerSubmissionActive)
            {
                // We also determine if the puzzle should be set to email-only mode.
                if (IsPuzzleSubmissionLimitReached(
                        thisEvent,
                        submissions,
                        puzzleState))
                {
                    if (!puzzle.IsForSinglePlayer)
                    {
                        await PuzzleStateHelper.SetEmailOnlyModeAsync(context,
                            thisEvent,
                            submission.Puzzle,
                            team,
                            true);
                    }
                    else
                    {
                        await SinglePlayerPuzzleStateHelper.SetEmailOnlyModeAsync(context,
                            thisEvent,
                            submission.Puzzle.ID,
                            submission.Submitter.ID,
                            true);
                    }

                    var authors = await context.PuzzleAuthors.Where((pa) => pa.Puzzle == submission.Puzzle).Select((pa) => pa.Author.Email).ToListAsync();
                    string affectedEntity = (!puzzle.IsForSinglePlayer) ? $"Team {team.Name}" : $"User {loggedInUser.Name ?? loggedInUser.Email}";
                    MailHelper.Singleton.SendPlaintextBcc(authors,
                        $"{thisEvent.Name}: {affectedEntity} is in email mode for {submission.Puzzle.Name}",
                        "");
                }
                else
                {
                    // If the submission was incorrect and not a partial solution,
                    // we will do the lockout computations now.
                    DateTime? lockoutExpiryTime = ComputeLockoutExpiryTime(
                        thisEvent,
                        submissions);

                    if (lockoutExpiryTime != null)
                    {
                        if (!puzzle.IsForSinglePlayer)
                        {
                            await PuzzleStateHelper.SetLockoutExpiryTimeAsync(context,
                                thisEvent,
                                submission.Puzzle,
                                team,
                                lockoutExpiryTime);
                        }
                        else
                        {
                            await SinglePlayerPuzzleStateHelper.SetLockoutExpiryTimeAsync(context,
                                thisEvent,
                                submission.Puzzle.ID,
                                loggedInUser.ID,
                                lockoutExpiryTime);
                        }
                    }
                }
            }
            Submission teamSubmission = submission as Submission;
            SinglePlayerPuzzleSubmission playerSubmission = submission as SinglePlayerPuzzleSubmission;
            if (teamSubmission != null)
            {
                context.Submissions.Add(teamSubmission);
            }
            else if (playerSubmission != null) 
            {
                context.SinglePlayerPuzzleSubmissions.Add(playerSubmission);
            }
            await context.SaveChangesAsync();

            // Send back responses for cases where the database has been updated
            // Correct response
            if (submission.Response != null && submission.Response.IsSolution)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Correct, CompleteResponse = submission.Response.ResponseText };
            }

            // Partial response
            if (submission.Response != null && !submission.Response.IsSolution)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Partial, CompleteResponse = submission.Response.ResponseText };
            }

            // Freeform response
            if (puzzle.IsFreeform)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Freeform, FreeformResponse = submission.FreeformResponse };
            }

            // Default to incorrect
            return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Incorrect };
        }
                
        /// <summary>
        /// Evaulates player submissions then either saves them to the database or returns an error to the caller
        /// This method is designed for use by admins and bypasses checks on the submission
        /// </summary>
        /// <param name="submittingUser">The user who will be credited with the submission</param>
        /// <param name="submissionTime">The timestap for the submission, DateTime.UtcNow if null</param>
        /// <returns></returns>
        public static async Task<SubmissionResponse> EvaluateSubmissionAdmin(PuzzleServerContext context, PuzzleUser submittingUser, Event thisEvent, int puzzleId, string submissionText, bool allowFreeformSharing, DateTime? submissionTime, string submitterDisplayName = "")
        {
            //Query data needed to process submission
            Puzzle puzzle = await context.Puzzles.Where(
                (p) => p.ID == puzzleId).FirstOrDefaultAsync();

            // Check if the submission is a duplicate
            string formattedSubmission = Response.FormatSubmission(submissionText);

            Team team = null;
            if (!puzzle.IsForSinglePlayer)
            {
                team = await UserEventHelper.GetTeamForPlayer(context, thisEvent, submittingUser);

                bool isDuplicate = await (from sub in context.Submissions
                                          where sub.Puzzle == puzzle &&
                                          sub.Team == team &&
                                          sub.SubmissionText == formattedSubmission
                                          select sub.SubmissionText).AnyAsync();

                if (isDuplicate)
                {
                    return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.DuplicateSubmission };
                }
            }
            else
            {
                bool isDuplicate = await (from sub in context.Submissions
                                          where sub.Puzzle == puzzle &&
                                          sub.Submitter == submittingUser &&
                                          sub.SubmissionText == formattedSubmission
                                          select sub.SubmissionText).AnyAsync();

                if (isDuplicate)
                {
                    return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.DuplicateSubmission };
                }
            }

                // Create submission
                SubmissionBase submission = (!puzzle.IsForSinglePlayer)
                    ? new Submission
                    {
                        TimeSubmitted = submissionTime == null ? DateTime.UtcNow : submissionTime.Value,
                        Puzzle = puzzle,
                        Team = team,
                        Submitter = submittingUser,
                        SubmitterDisplayName = submitterDisplayName,
                        AllowFreeformSharing = allowFreeformSharing
                    }
                    : new SinglePlayerPuzzleSubmission
                    {
                        TimeSubmitted = submissionTime == null ? DateTime.UtcNow : submissionTime.Value,
                        Puzzle = puzzle,
                        Submitter = submittingUser,
                        SubmitterDisplayName = submitterDisplayName,
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

            // Update puzzle state if submission was correct
            if (submission.Response != null && submission.Response.IsSolution)
            {
                if (!puzzle.IsForSinglePlayer)
                {
                    await PuzzleStateHelper.SetSolveStateAsync(context,
                        thisEvent,
                        submission.Puzzle,
                        team,
                        submission.TimeSubmitted);
                }
                else
                {
                    await SinglePlayerPuzzleStateHelper.SetSolveStateAsync(context,
                        thisEvent,
                        submission.Puzzle,
                        submittingUser.ID,
                        submission.TimeSubmitted);
                }
            }

            Submission teamSubmission = submission as Submission;
            SinglePlayerPuzzleSubmission playerSubmission = submission as SinglePlayerPuzzleSubmission;
            if (teamSubmission != null)
            {
                context.Submissions.Add(teamSubmission);
            }
            else if (playerSubmission != null)
            {
                context.SinglePlayerPuzzleSubmissions.Add(playerSubmission);
            }

            await context.SaveChangesAsync();

            // Send back responses for cases where the database has been updated
            // Correct response
            if (submission.Response != null && submission.Response.IsSolution)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Correct, CompleteResponse = submission.Response.ResponseText };
            }

            // Partial response
            if (submission.Response != null && !submission.Response.IsSolution)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Partial, CompleteResponse = submission.Response.ResponseText };
            }

            // Freeform response
            if (puzzle.IsFreeform)
            {
                return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Freeform, FreeformResponse = submission.FreeformResponse };
            }

            // Default to incorrect
            return new SubmissionResponse() { ResponseCode = SubmissionResponseCode.Incorrect };
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
            IEnumerable<SubmissionBase> submissions)
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
