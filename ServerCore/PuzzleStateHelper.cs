using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ServerMessages;

namespace ServerCore
{
    public class PuzzleStateHelper
    {
        public static IServiceProvider ServiceProvider;

        /// <summary>
        /// Get a read-only query of puzzle state. You won't be able to write to these values, but the query will be resilient to state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">The puzzle; if null, get all puzzles in the event.</param>
        /// <param name="team">The team; if null, get all the teams in the event.</param>
        /// <param name="author">The author; if null get puzzles matching other criteria by all authors</param>
        /// <returns>A query of PuzzleStatePerTeam objects that can be sorted and instantiated, but you can't edit the results.</returns>
        public static IQueryable<PuzzleStatePerTeam> GetFullReadOnlyQuery(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team, PuzzleUser author = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException("Context required.");
            }

            if (eventObj == null)
            {
                throw new ArgumentNullException("Event required.");
            }

            if (puzzle != null && team != null)
            {
                return context.PuzzleStatePerTeam
                    .Where(state => state.Puzzle == puzzle && state.Team == team);
            }

            if (puzzle != null)
            {
                return context.PuzzleStatePerTeam.Where(state => state.Puzzle == puzzle);
            }

            if (team != null)
            {
                if (author != null)
                {
                    return from state in context.PuzzleStatePerTeam
                           join auth in context.PuzzleAuthors on state.PuzzleID equals auth.PuzzleID
                           where state.Team == team &&
                           auth.Author == author
                           select state;
                }
                else
                {
                    return context.PuzzleStatePerTeam.Where(state => state.Team == team);
                }
            }

            throw new NotImplementedException("Full event query is NYI and may never be needed; use the sparse one");
        }

        /// <summary>
        /// Get a read-write query of puzzle state without any missing values. This is the most performant query, but its data is incomplete. Use with caution!
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">The puzzle; if null, get all puzzles in the event.</param>
        /// <param name="team">The team; if null, get all the teams in the event.</param>
        /// <returns>A query of PuzzleStatePerTeam objects that currently exist in the table.</returns>
        public static IQueryable<PuzzleStatePerTeam> GetSparseQuery(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team, PuzzleUser author = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException("Context required.");
            }

            if (eventObj == null)
            {
                throw new ArgumentNullException("Event required.");
            }

            if (puzzle != null && team != null)
            {
                return context.PuzzleStatePerTeam.Where(state => state.Puzzle == puzzle && state.Team == team);
            }

            if (puzzle != null)
            {
                return context.PuzzleStatePerTeam.Where(state => state.Puzzle == puzzle);
            }

            if (team != null)
            {
                if (author != null)
                {
                    return from state in context.PuzzleStatePerTeam
                           join auth in context.PuzzleAuthors on state.PuzzleID equals auth.PuzzleID
                           where state.Team == team &&
                           auth.Author == author
                           select state;
                }
                else
                {
                    return context.PuzzleStatePerTeam.Where(state => state.Team == team);
                }
            }

            return context.PuzzleStatePerTeam.Where(state => state.Puzzle.Event == eventObj);
        }

        /// <summary>
        /// Set the unlock state of some puzzle state records. In the course of setting the state, instantiate any state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">The puzzle; if null, get all puzzles in the event.</param>
        /// <param name="team">The team; if null, get all the teams in the event.</param>
        /// <param name="value">The unlock time (null if relocking)</param>
        /// <returns>A task that can be awaited for the unlock/lock operation</returns>
        public static async Task SetUnlockStateAsync(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team, DateTime? value, PuzzleUser author = null)
        {
            IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper.GetFullReadWriteQuery(context, eventObj, puzzle, team, author);
            List<PuzzleStatePerTeam> states = await statesQ.ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                // Only allow unlock time to be modified if we were relocking it (setting it to null) or unlocking it for the first time
                if (value == null || states[i].UnlockedTime == null)
                {
                    states[i].UnlockedTime = value;
                }
            }
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Set the solve state of some puzzle state records. In the course of setting the state, instantiate any state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">
        ///     The puzzle; if null, get all puzzles in the event.
        /// </param>
        /// <param name="team">
        ///     The team; if null, get all the teams in the event.
        /// </param>
        /// <param name="value">The solve time (null if unsolving)</param>
        /// <param name="author"></param>
        /// <returns>
        ///     A task that can be awaited for the solve/unsolve operation
        /// </returns>
        public static async Task SetSolveStateAsync(
            PuzzleServerContext context,
            Event eventObj,
            Puzzle puzzle,
            Team team,
            DateTime? value,
            PuzzleUser author = null)
        {
            IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper
                .GetFullReadWriteQuery(context, eventObj, puzzle, team, author);

            List<PuzzleStatePerTeam> states = await statesQ.ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                // Only allow solved time to be modified if it is being marked as unsolved (set to null) or if it is being solved for the first time
                if (value == null || states[i].SolvedTime == null)
                {
                    // Unlock puzzles when solving them
                    if (value != null && states[i].UnlockedTime == null)
                    {
                        states[i].UnlockedTime = value;
                    }

                    states[i].SolvedTime = value;
                }
            }

            // Award hint coins
            if (value != null && puzzle != null && puzzle.HintCoinsForSolve != 0)
            {
                if (team != null)
                {
                    team.HintCoinCount += puzzle.HintCoinsForSolve;
                }
                else
                {
                    var allTeams = from Team curTeam in context.Teams
                                   where curTeam.Event == eventObj
                                   select curTeam;
                    foreach(Team curTeam in allTeams)
                    {
                        curTeam.HintCoinCount += puzzle.HintCoinsForSolve;
                    }
                }
            }

            await context.SaveChangesAsync();

            // if this puzzle got solved, look for others to unlock
            if (puzzle != null && value != null)
            {
                // only send this notification when puzzles are embedded; otherwise, the notification is sent when there are no pages connected!
                if (eventObj.EmbedPuzzles && puzzle.IsPuzzle)
                {
                    await ServiceProvider.GetRequiredService<IHubContext<ServerMessageHub>>().SendNotification(team, "Puzzle solved!", $"{puzzle.Name} has been solved!", $"/{puzzle.Event.EventID}/play/Submissions/{puzzle.ID}");
                }

                await UnlockAnyPuzzlesThatThisSolveUnlockedAsync(context,
                    eventObj,
                    puzzle,
                    team,
                    value.Value);
            }
        }

        /// <summary>
        /// Set the email only mode of some puzzle state records. In the course
        /// of setting the state, instantiate any state records that are
        /// missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">
        ///     The puzzle; if null, get all puzzles in the event.
        /// </param>
        /// <param name="team">
        ///     The team; if null, get all the teams in the event.
        /// </param>
        /// <param name="value">The new email only state for the puzzle</param>
        /// <param name="author"></param>
        /// <returns>
        ///     A task that can be awaited for the lockout operation
        /// </returns>
        public static async Task SetEmailOnlyModeAsync(
            PuzzleServerContext context,
            Event eventObj,
            Puzzle puzzle,
            Team team,
            bool value,
            PuzzleUser author = null)
        {
            IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper
                .GetFullReadWriteQuery(context, eventObj, puzzle, team, author);

            List<PuzzleStatePerTeam> states = await statesQ.ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                states[i].IsEmailOnlyMode = value;
                if (value == true)
                {
                    states[i].WrongSubmissionCountBuffer += eventObj.MaxSubmissionCount;
                }
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Set the lockout expiry time of some puzzle state records. In the
        /// course of setting the state, instantiate any state records that are
        /// missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">
        ///     The puzzle; if null, get all puzzles in the event.
        /// </param>
        /// <param name="team">
        ///     The team; if null, get all the teams in the event.
        /// </param>
        /// <param name="value">The Lockout expiry time<param>
        /// <param name="author"></param>
        /// <returns>
        ///     A task that can be awaited for the lockout operation
        /// </returns>
        public static async Task SetLockoutExpiryTimeAsync(
            PuzzleServerContext context,
            Event eventObj,
            Puzzle puzzle,
            Team team,
            DateTime? value,
            PuzzleUser author = null)
        {
            IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper
                .GetFullReadWriteQuery(context, eventObj, puzzle, team, author);

            List<PuzzleStatePerTeam> states = await statesQ.ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                states[i].LockoutExpiryTime = value;
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Get the solved status of a single puzzle.
        /// Do not use if you want to get status of many puzzles as it will be very inefficient.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> IsPuzzleSolved(PuzzleServerContext context, int puzzleID, int teamID)
        {
            DateTime? solved = await (from PuzzleStatePerTeam pspt in context.PuzzleStatePerTeam
                                      where pspt.PuzzleID == puzzleID && pspt.TeamID == teamID
                                      select pspt.SolvedTime).FirstOrDefaultAsync();
            return solved != null;
        }

        private static DateTime LastGlobalExpiry;
        private static Dictionary<int, DateTime> TimedUnlockExpiryCache = new Dictionary<int, DateTime>();
        private static TimeSpan ClosestExpirySpacing = TimeSpan.FromSeconds(2);

        public static async Task CheckForTimedUnlocksAsync(
            PuzzleServerContext context,
            Event eventObj,
            Team team)
        {
            DateTime expiry;
            DateTime now = DateTime.UtcNow;

            lock (TimedUnlockExpiryCache)
            {
                // throttle this by an expiry interval before we do anything even remotely expensive
                if (TimedUnlockExpiryCache.TryGetValue(team.ID, out expiry) && expiry >= now)
                {
                    return;
                }
            }

            // unlock any puzzle with zero prerequisites
            var zeroPrerequisitePuzzlesToUnlock = await PuzzleStateHelper.GetSparseQuery(context, eventObj, null, team)
                .Where(state => state.UnlockedTime == null && state.Puzzle.MinPrerequisiteCount == 0)
                .Select((state) => state.Puzzle)
                .ToListAsync();
            foreach (var puzzle in zeroPrerequisitePuzzlesToUnlock)
            {
                await PuzzleStateHelper.SetUnlockStateAsync(context, eventObj, puzzle, team, eventObj.EventBegin);
            }

            // do the unlocks in a loop.
            // The loop will catch cascading unlocks, e.g. if someone does not hit the site between 11:59 and 12:31, catch up to the 12:30 unlocks immediately.
            while (true)
            {
                var puzzlesToSolveByTime = await PuzzleStateHelper.GetSparseQuery(context, eventObj, null, team)
                    .Where(state => state.SolvedTime == null && state.UnlockedTime != null && state.Puzzle.MinutesToAutomaticallySolve != null && EF.Functions.DateDiffMinute(state.UnlockedTime.Value, now) >= state.Puzzle.MinutesToAutomaticallySolve.Value)
                    .Select((state) => new { Puzzle = state.Puzzle, UnlockedTime = state.UnlockedTime.Value })
                    .ToListAsync();

                foreach (var state in puzzlesToSolveByTime)
                {
                    // mark solve time as when the puzzle was supposed to complete, so cascading unlocks work properly.
                    await PuzzleStateHelper.SetSolveStateAsync(context, eventObj, state.Puzzle, team, state.UnlockedTime + TimeSpan.FromMinutes(state.Puzzle.MinutesToAutomaticallySolve.Value));
                }

                // get out of the loop if we did nothing
                if (puzzlesToSolveByTime.Count == 0)
                {
                    break;
                }
            }

            lock (TimedUnlockExpiryCache)
            {
                // effectively, expiry = Math.Max(now, LastGlobalExpiry) + ClosestExpirySpacing - if you could use Math.Max on DateTime
                expiry = now;
                if (expiry < LastGlobalExpiry)
                {
                    expiry = LastGlobalExpiry;
                }
                expiry += ClosestExpirySpacing;

                TimedUnlockExpiryCache[team.ID] = expiry;
                LastGlobalExpiry = expiry;
            }
        }

        public static IQueryable<Puzzle> PuzzlesCausingGlobalLockout(
            PuzzleServerContext context,
            Event eventObj,
            Team team)
        {
            DateTime now = DateTime.UtcNow;
            return PuzzleStateHelper.GetSparseQuery(context, eventObj, null, team)
                .Where(state => state.SolvedTime == null && state.UnlockedTime != null && state.Puzzle.MinutesOfEventLockout != 0 && EF.Functions.DateDiffMinute(state.UnlockedTime.Value, now) < state.Puzzle.MinutesOfEventLockout)
                .Select((s) => s.Puzzle);
        }

        /// <summary>
        /// Get a writable query of puzzle state. In the course of constructing the query, it will instantiate any state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">The puzzle; if null, get all puzzles in the event.</param>
        /// <param name="team">The team; if null, get all the teams in the event.</param>
        /// <returns>A query of PuzzleStatePerTeam objects that can be sorted and instantiated, but you can't edit the results.</returns>
        private static IQueryable<PuzzleStatePerTeam> GetFullReadWriteQuery(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team, PuzzleUser author)
        {
            return GetSparseQuery(context, eventObj, puzzle, team, author);
        }

        /// <summary>
        /// Unlock any puzzles that need to be unlocked due to the recent solve of a prerequisite.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are working in</param>
        /// <param name="puzzleJustSolved">The puzzle just solved</param>
        /// <param name="team">The team that just solved; if null, all the teams in the event.</param>
        /// <param name="unlockTime">The time that the puzzle should be marked as unlocked.</param>
        /// <returns></returns>
        private static async Task UnlockAnyPuzzlesThatThisSolveUnlockedAsync(PuzzleServerContext context, Event eventObj, Puzzle puzzleJustSolved, Team team, DateTime unlockTime)
        {
            var puzzlesInGroup = puzzleJustSolved.Group == null ? null : await context.Puzzles.Where(p => p.Event == eventObj && !p.IsForSinglePlayer && p.Group == puzzleJustSolved.Group).Select(p => new { PuzzleID = p.ID, MinInGroupCount = p.MinInGroupCount, IsPuzzle = p.IsPuzzle }).ToDictionaryAsync(p => p.PuzzleID);

            // get the prerequisites for all puzzles that need an update
            // information we get per puzzle: { id, min count, number of solved prereqs including this one }
            var prerequisiteDataForNeedsUpdatePuzzles = (from possibleUnlock in context.Prerequisites
                                                        join unlockedBy in context.Prerequisites on possibleUnlock.PuzzleID equals unlockedBy.PuzzleID
                                                        join pspt in context.PuzzleStatePerTeam on unlockedBy.PrerequisiteID equals pspt.PuzzleID
                                                        join puz in context.Puzzles on unlockedBy.PrerequisiteID equals puz.ID
                                                        where possibleUnlock.Prerequisite == puzzleJustSolved && !possibleUnlock.Puzzle.IsForSinglePlayer && !puz.IsForSinglePlayer && (team == null || pspt.TeamID == team.ID) && pspt.SolvedTime != null
                                                        group puz by new { unlockedBy.PuzzleID, unlockedBy.Puzzle.MinPrerequisiteCount, unlockedBy.Puzzle.IsPuzzle, pspt.TeamID } into g
                                                        select new
                                                        {
                                                            PuzzleID = g.Key.PuzzleID,
                                                            TeamID = g.Key.TeamID,
                                                            g.Key.MinPrerequisiteCount,
                                                            g.Key.IsPuzzle,
                                                            TotalPrerequisiteCount = g.Sum(p => (p.PrerequisiteWeight ?? 1))
                                                        }).ToList();

            // Are we updating one team or all teams?
            List<Team> teamsToUpdate = team == null ? await context.Teams.Where(t => t.Event == eventObj).ToListAsync() : new List<Team>() { team };

            // Update teams one at a time
            foreach (Team t in teamsToUpdate)
            {
                HashSet<int> puzzlesUnlockedToNotify = new HashSet<int>();

                // Collect the IDs of all solved/unlocked puzzles for this team
                // sparse lookup is fine since if the state is missing it isn't unlocked or solved!
                var puzzleStateForTeamT = await PuzzleStateHelper.GetSparseQuery(context, eventObj, null, t)
                    .Select(state => new { state.PuzzleID, state.UnlockedTime, state.SolvedTime })
                    .ToListAsync();

                // Make a hash set out of them for easy lookup in case we have several prerequisites to chase
                HashSet<int> unlockedPuzzleIDsForTeamT = new HashSet<int>();
                int solveCountInGroup = 0;

                foreach (var puzzleState in puzzleStateForTeamT)
                {
                    if (puzzleState.UnlockedTime != null)
                    {
                        unlockedPuzzleIDsForTeamT.Add(puzzleState.PuzzleID);
                    }

                    if (puzzleState.SolvedTime != null && puzzlesInGroup != null && puzzlesInGroup.ContainsKey(puzzleState.PuzzleID))
                    {
                        solveCountInGroup++;
                    }
                }

                // now loop through all puzzles and count up who needs to be unlocked
                foreach (var puzzleToUpdate in prerequisiteDataForNeedsUpdatePuzzles)
                {
                    // already unlocked? skip
                    if (unlockedPuzzleIDsForTeamT.Contains(puzzleToUpdate.PuzzleID))
                    {
                        continue;
                    }

                    // Enough puzzles unlocked by count? Let's unlock it
                    if (puzzleToUpdate.TeamID == t.ID && puzzleToUpdate.TotalPrerequisiteCount >= puzzleToUpdate.MinPrerequisiteCount)
                    {
                        PuzzleStatePerTeam state = await context.PuzzleStatePerTeam.Where(s => s.PuzzleID == puzzleToUpdate.PuzzleID && s.Team == t).FirstAsync();
                        state.UnlockedTime = unlockTime;

                        if (puzzleToUpdate.IsPuzzle)
                        {
                            puzzlesUnlockedToNotify.Add(puzzleToUpdate.PuzzleID);
                        }
                    }
                }

                if (puzzlesInGroup != null)
                {
                    foreach (var pair in puzzlesInGroup)
                    {
                        if (pair.Value.MinInGroupCount.HasValue &&
                            solveCountInGroup >= pair.Value.MinInGroupCount.Value &&
                            !unlockedPuzzleIDsForTeamT.Contains(pair.Key))
                        {
                            // enough puzzles unlocked in the same group? Let's unlock it
                            PuzzleStatePerTeam state = await context.PuzzleStatePerTeam.Where(s => s.PuzzleID == pair.Key && s.Team == t).FirstAsync();
                            state.UnlockedTime = unlockTime;
                            if (pair.Value.IsPuzzle)
                            {
                                puzzlesUnlockedToNotify.Add(pair.Key);
                            }
                        }
                    }
                }

                // only send these notifications when puzzles are embedded; otherwise, the notifications are sent when there are no pages connected!
                if (eventObj.EmbedPuzzles)
                {
                    if (puzzlesUnlockedToNotify.Count > 3)
                    {
                        await ServiceProvider.GetRequiredService<IHubContext<ServerMessageHub>>().SendNotification(t, "New puzzles!", $"{puzzlesUnlockedToNotify.Count} puzzles have been unlocked!", $"/{eventObj.EventID}/play/Play");
                    }
                    else if (puzzlesUnlockedToNotify.Count > 0)
                    {
                        var puzzles = await context.Puzzles.Where(p => puzzlesUnlockedToNotify.Contains(p.ID)).ToListAsync();

                        foreach (var puzzle in puzzles)
                        {
                            await ServiceProvider.GetRequiredService<IHubContext<ServerMessageHub>>().SendNotification(t, "New puzzle!", $"{puzzle.Name} has been unlocked!", $"/{eventObj.EventID}/play/Submissions/{puzzle.ID}");
                        }
                    }
                }
            }

            // after looping through all teams, send one update with all changes made
            await context.SaveChangesAsync();
        }
        public static async Task UpdateTeamsWhoSentResponse(PuzzleServerContext context, Response response)
        {
            using (IDbContextTransaction transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                var submissionsThatMatchResponse = await (from pspt in context.PuzzleStatePerTeam
                                                          join sub in context.Submissions on pspt.Team equals sub.Team
                                                          where pspt.PuzzleID == response.PuzzleID &&
                                                          sub.PuzzleID == response.PuzzleID &&
                                                          sub.SubmissionText == response.SubmittedText
                                                          select new { State = pspt, Submission = sub }).ToListAsync();

                if (submissionsThatMatchResponse.Count > 0)
                {
                    Puzzle puzzle = await context.Puzzles.Where((p) => p.ID == response.PuzzleID).FirstOrDefaultAsync();

                    foreach (var s in submissionsThatMatchResponse)
                    {
                        s.Submission.Response = response;
                        context.Attach(s.Submission).State = EntityState.Modified;

                        if (response.IsSolution && s.State.SolvedTime == null)
                        {
                            await SetSolveStateAsync(context, puzzle.Event, puzzle, s.State.Team, s.Submission.TimeSubmitted);
                        }
                    }

                    await context.SaveChangesAsync();
                    transaction.Commit();

                    var teamMembers = await (from tm in context.TeamMembers
                                             join sub in context.Submissions on tm.Team equals sub.Team
                                             where sub.PuzzleID == response.PuzzleID && sub.SubmissionText == response.SubmittedText
                                             select tm.Member.Email).ToListAsync();
                    var teams = await (from sub in context.Submissions
                                             where sub.PuzzleID == response.PuzzleID && sub.SubmissionText == response.SubmittedText
                                             select sub.Team).ToListAsync();
                    var plaintextResponseText = response.GetPlaintextResponseText(puzzle?.EventID ?? 0);
                    MailHelper.Singleton.SendPlaintextBcc(teamMembers,
                        $"{puzzle.Event.Name}: {puzzle.PlaintextName} Response updated for '{response.SubmittedText}'",
                        $"The new response for this submission is: '{plaintextResponseText}'.");
                    foreach (Team team in teams)
                    {
                        await ServiceProvider.GetRequiredService<IHubContext<ServerMessageHub>>().SendNotification(team, $"{puzzle.PlaintextName} Response updated for '{response.SubmittedText}'", $"The new response for this submission is: '{plaintextResponseText}'.", $"/{puzzle.Event.EventID}/play/Submissions/{puzzle.ID}");
                    }
                }
            }
        }
    }
}
