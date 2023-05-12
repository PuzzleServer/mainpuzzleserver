using ServerCore.DataModel;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;

namespace ServerCore
{
    public class SinglePlayerPuzzleStateHelper
    {
        /// <summary>
        /// Get a read-only query of puzzle state. You won't be able to write to these values, but the query will be resilient to state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzleId">The puzzle id; if null, get all puzzles in the event.</param>
        /// <param name="playerId">The player id; if null, get all the players in the event.</param>
        /// <param name="author">The author; if null get puzzles matching other criteria by all authors</param>
        /// <returns>A query of PuzzleStatePerTeam objects that can be sorted and instantiated, but you can't edit the results.</returns>
        public static IQueryable<SinglePlayerPuzzleStatePerPlayer> GetFullReadOnlyQuery(PuzzleServerContext context, Event eventObj, int? puzzleId, int? playerId, PuzzleUser author = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException("Context required.");
            }

            if (eventObj == null)
            {
                throw new ArgumentNullException("Event required.");
            }

            if (puzzleId.HasValue && playerId.HasValue)
            {
                return context.SinglePlayerPuzzleStatePerPlayer
                    .Where(state => state.PuzzleID == puzzleId && state.UserID == playerId);
            }

            if (puzzleId.HasValue)
            {
                return context.SinglePlayerPuzzleStatePerPlayer.Where(state => state.PuzzleID == puzzleId);
            }

            if (playerId.HasValue)
            {
                if (author != null)
                {
                    return from state in context.SinglePlayerPuzzleStatePerPlayer
                           join auth in context.PuzzleAuthors on state.PuzzleID equals auth.PuzzleID
                           where state.UserID == playerId && auth.Author == author
                           select state;
                }
                else
                {
                    return context.SinglePlayerPuzzleStatePerPlayer.Where(state => state.UserID == playerId);
                }
            }

            throw new NotImplementedException("Full event query is NYI and may never be needed; use the sparse one");
        }

        /// <summary>
        /// Get a read-write query of puzzle state without any missing values. This is the most performant query, but its data is incomplete. Use with caution!
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzleId">The puzzle id; if null, get all puzzles in the event.</param>
        /// <param name="playerId">The player id; if null, get all the players in the event.</param>
        /// <returns>A query of SinglePlayerPuzzleStatePerPlayer objects that currently exist in the table.</returns>
        public static IQueryable<SinglePlayerPuzzleStatePerPlayer> GetSparseQuery(PuzzleServerContext context, Event eventObj, int? puzzleId, int? playerId, PuzzleUser author = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException("Context required.");
            }

            if (eventObj == null)
            {
                throw new ArgumentNullException("Event required.");
            }

            if (puzzleId.HasValue && playerId.HasValue)
            {
                return context.SinglePlayerPuzzleStatePerPlayer.Where(state => state.PuzzleID == puzzleId && state.UserID == playerId);
            }

            if (puzzleId.HasValue)
            {
                return context.SinglePlayerPuzzleStatePerPlayer.Where(state => state.PuzzleID == puzzleId);
            }

            if (playerId.HasValue)
            {
                if (author != null)
                {
                    return from state in context.SinglePlayerPuzzleStatePerPlayer
                           join auth in context.PuzzleAuthors on state.PuzzleID equals auth.PuzzleID
                           where state.UserID == playerId &&
                           auth.Author == author
                           select state;
                }
                else
                {
                    return context.SinglePlayerPuzzleStatePerPlayer.Where(state => state.UserID == playerId);
                }
            }

            return context.SinglePlayerPuzzleStatePerPlayer.Where(state => state.Puzzle.Event == eventObj);
        }

        /// <summary>
        /// Set the unlock state of some puzzle state records. In the course of setting the state, instantiate any state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzleId">The puzzle id; if null, get all puzzles in the event.</param>
        /// <param name="playerId">The player id; if null, get all the players in the event.</param>
        /// <param name="value">The unlock time (null if relocking)</param>
        /// <returns>A task that can be awaited for the unlock/lock operation</returns>
        public static async Task SetUnlockStateAsync(PuzzleServerContext context, Event eventObj, int? puzzleId, int? playerId, DateTime? value, PuzzleUser author = null)
        {
            IQueryable<SinglePlayerPuzzleStatePerPlayer> statesQ = SinglePlayerPuzzleStateHelper.GetFullReadWriteQuery(context, eventObj, puzzleId, playerId, author);
            List<SinglePlayerPuzzleStatePerPlayer> states = await statesQ.ToListAsync();

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

        public static IQueryable<Puzzle> PuzzlesCausingGlobalLockout(
            PuzzleServerContext context,
            Event eventObj,
            int playerId)
        {
            DateTime now = DateTime.UtcNow;
            return SinglePlayerPuzzleStateHelper.GetSparseQuery(context, eventObj, puzzleId: null, playerId: playerId)
                .Where(state => state.SolvedTime == null && state.UnlockedTime != null && state.Puzzle.MinutesOfEventLockout != 0 && EF.Functions.DateDiffMinute(state.UnlockedTime.Value, now) < state.Puzzle.MinutesOfEventLockout)
                .Select((s) => s.Puzzle);
        }

        /// <summary>
        /// Get a writable query of puzzle state. In the course of constructing the query, it will instantiate any state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzleId">The puzzle id; if null, get all puzzles in the event.</param>
        /// <param name="playerId">The player id; if null, get all the states of every player in the event.</param>
        /// <returns>A query of SinglePlayerPuzzleStatePerPlayer objects that can be sorted and instantiated, but you can't edit the results.</returns>
        private static IQueryable<SinglePlayerPuzzleStatePerPlayer> GetFullReadWriteQuery(PuzzleServerContext context, Event eventObj, int? puzzleId, int? playerId, PuzzleUser author)
        {
            return GetSparseQuery(context, eventObj, puzzleId, playerId, author);
        }

        /// <summary>
        /// Set the solve state of some puzzle state records. In the course of setting the state, instantiate any state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzleId">
        ///     The puzzle id; if null, get all puzzles in the event.
        /// </param>
        /// <param name="playerId">
        ///     The player id. If null, get all the submissions for the puzzle in the event.
        /// </param>
        /// <param name="value">The solve time (null if unsolving)</param>
        /// <param name="author"></param>
        /// <returns>
        ///     A task that can be awaited for the solve/unsolve operation
        /// </returns>
        public static async Task SetSolveStateAsync(
            PuzzleServerContext context,
            Event eventObj,
            int? puzzleId,
            int? playerId,
            DateTime? value,
            PuzzleUser author = null)
        {
            IQueryable<SinglePlayerPuzzleStatePerPlayer> statesQ = SinglePlayerPuzzleStateHelper
                .GetFullReadWriteQuery(context, eventObj, puzzleId, playerId, author);

            List<SinglePlayerPuzzleStatePerPlayer> states = await statesQ.ToListAsync();

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

            await context.SaveChangesAsync();

            // if this puzzle got solved, look for others to unlock
            if (puzzleId.HasValue && value != null)
            {
                await UnlockAnyPuzzlesThatThisSolveUnlockedAsync(context,
                    eventObj,
                    puzzleId,
                    playerId,
                    value.Value);
            }
        }

        /// <summary>
        /// Unlock any puzzles that need to be unlocked due to the recent solve of a prerequisite.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are working in</param>
        /// <param name="puzzleJustSolvedId">The id of the puzzle just solved</param>
        /// <param name="playerId">The id of the player.</param>
        /// <param name="unlockTime">The time that the puzzle should be marked as unlocked.</param>
        /// <returns></returns>
        private static async Task UnlockAnyPuzzlesThatThisSolveUnlockedAsync(PuzzleServerContext context, Event eventObj, int? puzzleJustSolvedId, int? playerId, DateTime unlockTime)
        {
            // a simple query for all puzzle IDs in the event - will be used at least once below
            IQueryable<int> allPuzzleIDsQ = context.Puzzles.Where(p => p.Event == eventObj).Select(p => p.ID);


            // get the prerequisites for all puzzles that need an update
            // information we get per puzzle: { id, min count, number of solved prereqs including this one }
            var prerequisiteDataForNeedsUpdatePuzzles = (from possibleUnlock in context.Prerequisites
                                                         join unlockedBy in context.Prerequisites on possibleUnlock.PuzzleID equals unlockedBy.PuzzleID
                                                         join pspt in context.SinglePlayerPuzzleStatePerPlayer on unlockedBy.PrerequisiteID equals pspt.PuzzleID
                                                         join puz in context.Puzzles on unlockedBy.PrerequisiteID equals puz.ID
                                                         where possibleUnlock.Prerequisite.ID == puzzleJustSolvedId && pspt.SolvedTime != null
                                                         group puz by new { unlockedBy.PuzzleID, unlockedBy.Puzzle.MinPrerequisiteCount, pspt.UserID } into g
                                                         select new
                                                         {
                                                             PuzzleID = g.Key.PuzzleID,
                                                             UserID = g.Key.UserID,
                                                             g.Key.MinPrerequisiteCount,
                                                             TotalPrerequisiteCount = g.Sum(p => (p.PrerequisiteWeight ?? 1))
                                                         }).ToList();

            // Collect the IDs of all solved/unlocked puzzles for this team
            // sparse lookup is fine since if the state is missing it isn't unlocked or solved!
            var singlePlayerPuzzleState = await SinglePlayerPuzzleStateHelper.GetSparseQuery(context, eventObj, puzzleId: null, playerId: playerId)
                .Select(state => new { state.PuzzleID, state.UnlockedTime, state.SolvedTime })
                .ToListAsync();

            // Make a hash set out of them for easy lookup in case we have several prerequisites to chase
            HashSet<int> unlockedPuzzleIDs = new HashSet<int>();
            foreach (var puzzleState in singlePlayerPuzzleState)
            {
                if (puzzleState.UnlockedTime != null)
                {
                    unlockedPuzzleIDs.Add(puzzleState.PuzzleID);
                }
            }

            // now loop through all puzzles and count up who needs to be unlocked
            foreach (var puzzleToUpdate in prerequisiteDataForNeedsUpdatePuzzles)
            {
                // already unlocked? skip
                if (unlockedPuzzleIDs.Contains(puzzleToUpdate.PuzzleID))
                {
                    continue;
                }

                // Enough puzzles unlocked by count? Let's unlock it
                if (puzzleToUpdate.TotalPrerequisiteCount >= puzzleToUpdate.MinPrerequisiteCount)
                {
                    SinglePlayerPuzzleStatePerPlayer state = await context.SinglePlayerPuzzleStatePerPlayer.Where(s => s.PuzzleID == puzzleToUpdate.PuzzleID && s.UserID == playerId).FirstAsync();
                    state.UnlockedTime = unlockTime;
                }
            }

            // after looping through all teams, send one update with all changes made
            await context.SaveChangesAsync();
        }


        /// <summary>
        /// Set the email only mode of some puzzle state records. In the course
        /// of setting the state, instantiate any state records that are
        /// missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzleId">
        ///     The puzzle id; if null, get all puzzles in the event.
        /// </param>
        /// <param name="playerId">
        ///     The player id; if null, get all the players in the event.
        /// </param>
        /// <param name="value">True if should be in email only mode.</param>
        /// <param name="author"></param>
        /// <returns>
        ///     A task that can be awaited for the lockout operation
        /// </returns>
        public static async Task SetEmailOnlyModeAsync(
            PuzzleServerContext context,
            Event eventObj,
            int? puzzleId,
            int playerId,
            bool value,
            PuzzleUser author = null)
        {
            IQueryable<SinglePlayerPuzzleStatePerPlayer> statesQ = SinglePlayerPuzzleStateHelper
                .GetFullReadWriteQuery(context, eventObj, puzzleId, playerId, author);

            List<SinglePlayerPuzzleStatePerPlayer> states = await statesQ.ToListAsync();

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
        /// <param name="puzzleId">
        ///     The puzzle id; if null, get all puzzles in the event.
        /// </param>
        /// <param name="playerId">
        ///     The player id; if null, get all the players in the event.
        /// </param>
        /// <param name="value">The Lockout expiry time<param>
        /// <param name="author">The author.</param>
        /// <returns>
        ///     A task that can be awaited for the lockout operation
        /// </returns>
        public static async Task SetLockoutExpiryTimeAsync(
            PuzzleServerContext context,
            Event eventObj,
            int? puzzleId,
            int? playerId,
            DateTime? value,
            PuzzleUser author = null)
        {
            IQueryable<SinglePlayerPuzzleStatePerPlayer> statesQ = SinglePlayerPuzzleStateHelper
                .GetFullReadWriteQuery(context, eventObj, puzzleId, playerId, author);

            List<SinglePlayerPuzzleStatePerPlayer> states = await statesQ.ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                states[i].LockoutExpiryTime = value;
            }

            await context.SaveChangesAsync();
        }
    }
}
