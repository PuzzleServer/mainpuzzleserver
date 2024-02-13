using ServerCore.DataModel;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                    .Where(state => state.PuzzleID == puzzleId && state.PlayerID == playerId);
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
                           where state.PlayerID == playerId && auth.Author == author
                           select state;
                }
                else
                {
                    return context.SinglePlayerPuzzleStatePerPlayer.Where(state => state.PlayerID == playerId);
                }
            }

            return context.SinglePlayerPuzzleStatePerPlayer.Where(state => state.Puzzle.Event == eventObj);
        }

        /// <summary>
        /// Get a read-write query of puzzle state without any missing values.
        /// This is the most performant query, but its data is incomplete. Use with caution!
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzleId">The puzzle id; if null, get all puzzles in the event.</param>
        /// <param name="playerId">The player id; if null, get all the players in the event.</param>
        /// <returns>A query of SinglePlayerPuzzleStatePerPlayer objects that currently exist in the table.</returns>
        public static IQueryable<SinglePlayerPuzzleStatePerPlayer> GetFullReadWriteQuery(PuzzleServerContext context, Event eventObj, int? puzzleId, int? playerId, PuzzleUser author = null)
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
                return from state in context.SinglePlayerPuzzleStatePerPlayer
                       where state.PlayerID == playerId && state.PuzzleID == puzzleId
                       select state;
            }

            if (puzzleId.HasValue)
            {
                return from state in context.SinglePlayerPuzzleStatePerPlayer
                       where state.PuzzleID == puzzleId
                       select state;
            }

            if (playerId.HasValue)
            {
                if (author != null)
                {
                    return from state in context.SinglePlayerPuzzleStatePerPlayer
                           join auth in context.PuzzleAuthors on state.PuzzleID equals auth.PuzzleID
                           where state.PlayerID == playerId &&
                           auth.Author == author
                           select state;
                }
                else
                {
                    return from state in context.SinglePlayerPuzzleStatePerPlayer
                           where state.PlayerID == playerId
                           select state;
                }
            }

            return from state in context.SinglePlayerPuzzleStatePerPlayer
                   where state.Puzzle.Event == eventObj
                   select state;
        }

        /// <summary>
        /// Set the solve state of some single player puzzle state records.
        /// In the course of setting the state, instantiate any state records that are missing on the server.
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
            Puzzle puzzle,
            int? playerId,
            DateTime? value,
            PuzzleUser author = null)
        {
            IQueryable<SinglePlayerPuzzleStatePerPlayer> statesQ = SinglePlayerPuzzleStateHelper
                .GetFullReadWriteQuery(context, eventObj, puzzle?.ID, playerId, author);

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

            // Award hint coins
            if (value != null && puzzle != null && puzzle.HintCoinsForSolve != 0)
            {
                IQueryable<PlayerInEvent> playersInEvent;
                if (playerId != null)
                {
                    playersInEvent = from PlayerInEvent player in context.PlayerInEvent
                                        where player.PlayerId == playerId && player.EventId == eventObj.ID
                                        select player;
                }
                else
                {
                    playersInEvent = from PlayerInEvent player in context.PlayerInEvent
                                     where player.EventId == eventObj.ID
                                     select player;
                }

                foreach (PlayerInEvent player in playersInEvent)
                {
                    player.HintCoinCount += puzzle.HintCoinsForSolve;
                }
            }

            await context.SaveChangesAsync();

            // if this puzzle got solved, look for others to unlock
            if (puzzle != null && value != null)
            {
                await UnlockAnyPuzzlesThatThisSolveUnlockedAsync(context,
                    eventObj,
                    puzzle,
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
        private static async Task UnlockAnyPuzzlesThatThisSolveUnlockedAsync(PuzzleServerContext context, Event eventObj, Puzzle puzzleJustSolved, int? playerId, DateTime unlockTime)
        {
            var puzzlesInGroup = puzzleJustSolved.Group == null ? null : await context.Puzzles.Where(p => p.Event == eventObj && p.IsForSinglePlayer && p.Group == puzzleJustSolved.Group).Select(p => new { PuzzleID = p.ID, MinInGroupCount = p.MinInGroupCount }).ToDictionaryAsync(p => p.PuzzleID);

            // get the prerequisites for all puzzles that need an update
            // information we get per puzzle: { id, min count, number of solved prereqs including this one }
            var prerequisiteDataForNeedsUpdatePuzzles = (from possibleUnlock in context.Prerequisites
                                                         join unlockedBy in context.Prerequisites on possibleUnlock.PuzzleID equals unlockedBy.PuzzleID
                                                         join pspt in context.SinglePlayerPuzzleStatePerPlayer on unlockedBy.PrerequisiteID equals pspt.PuzzleID
                                                         join puz in context.Puzzles on unlockedBy.PrerequisiteID equals puz.ID
                                                         where possibleUnlock.Prerequisite.ID == puzzleJustSolved.ID && pspt.SolvedTime != null
                                                         group puz by new { unlockedBy.PuzzleID, unlockedBy.Puzzle.MinPrerequisiteCount, pspt.PlayerID } into g
                                                         select new
                                                         {
                                                             PuzzleID = g.Key.PuzzleID,
                                                             UserID = g.Key.PlayerID,
                                                             g.Key.MinPrerequisiteCount,
                                                             TotalPrerequisiteCount = g.Sum(p => (p.PrerequisiteWeight ?? 1))
                                                         }).ToList();

            // Collect the IDs of all solved/unlocked puzzles for this team
            List<SinglePlayerPuzzleStatePerPlayer> singlePlayerPuzzleStates = await SinglePlayerPuzzleStateHelper
                .GetFullReadWriteQuery(context, eventObj, puzzleId: null, playerId: playerId)
                .ToListAsync();

            // Make a hash set out of them for easy lookup in case we have several prerequisites to chase
            HashSet<int> unlockedPuzzleIDs = singlePlayerPuzzleStates
                .Where(puzzleState => puzzleState.UnlockedTime != null)
                .Select(puzzleState => puzzleState.PuzzleID)
                .ToHashSet();

            int solveCountInGroup = puzzlesInGroup == null ? 0 : singlePlayerPuzzleStates
                .Where(puzzleState => puzzleState.SolvedTime != null)
                .Count();

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
                    SinglePlayerPuzzleStatePerPlayer state = await context.SinglePlayerPuzzleStatePerPlayer.Where(s => s.PuzzleID == puzzleToUpdate.PuzzleID && s.PlayerID == playerId).FirstAsync();
                    state.UnlockedTime = unlockTime;
                }
            }

            if (puzzlesInGroup != null)
            {
                foreach (var pair in puzzlesInGroup)
                {
                    if (pair.Value.MinInGroupCount.HasValue &&
                        solveCountInGroup >= pair.Value.MinInGroupCount.Value &&
                        !unlockedPuzzleIDs.Contains(pair.Key))
                    {
                        // enough puzzles unlocked in the same group? Let's unlock it
                        SinglePlayerPuzzleStatePerPlayer state = await context.SinglePlayerPuzzleStatePerPlayer.Where(s => s.PuzzleID == pair.Key && s.PlayerID == playerId).FirstAsync();
                        state.UnlockedTime = unlockTime;
                    }
                }
            }

            // after looping through all teams, send one update with all changes made
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Set the email only mode of some single player puzzle state records.
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
            int? playerId,
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

        public static IQueryable<Puzzle> PuzzlesCausingGlobalLockout(
            PuzzleServerContext context,
            Event eventObj,
            int playerId)
        {
            DateTime now = DateTime.UtcNow;
            return SinglePlayerPuzzleStateHelper.GetFullReadOnlyQuery(context, eventObj, puzzleId: null, playerId: playerId)
                .Where(state => state.SolvedTime == null && state.UnlockedTime != null && state.Puzzle.MinutesOfEventLockout != 0 && EF.Functions.DateDiffMinute(state.UnlockedTime.Value, now) < state.Puzzle.MinutesOfEventLockout)
                .Select((s) => s.Puzzle);
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

        public static async Task<SinglePlayerPuzzleStatePerPlayer> GetOrAddStateIfNotThere(
            PuzzleServerContext context,
            Event eventObj,
            Puzzle puzzle,
            int playerId)
        {
            IQueryable<SinglePlayerPuzzleStatePerPlayer> statesQ = SinglePlayerPuzzleStateHelper
                .GetFullReadWriteQuery(context, eventObj, puzzle.ID, playerId, author: null);
            SinglePlayerPuzzleStatePerPlayer state = statesQ.FirstOrDefault();
            if (state == null)
            {
                SinglePlayerPuzzleUnlockState unlockState = context.SinglePlayerPuzzleUnlockStates
                    .Where(state => state.Puzzle == puzzle)
                    .FirstOrDefault();

                state = new SinglePlayerPuzzleStatePerPlayer { Puzzle = puzzle, PlayerID = playerId, UnlockedTime = unlockState?.UnlockedTime };
                context.SinglePlayerPuzzleStatePerPlayer.Add(state);
                await context.SaveChangesAsync();
            }

            return state;
        }

        /// <summary>
        /// Set the unlock state of some puzzle state records.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzleId">The puzzle id; if null, get all puzzles in the event.</param>
        /// <param name="player">The team; if null, get all the players in the event.</param>
        /// <param name="value">The unlock time (null if relocking)</param>
        /// <returns>A task that can be awaited for the unlock/lock operation</returns>
        public static async Task SetUnlockStateAsync(PuzzleServerContext context, Event eventObj, int? puzzleId, int? playerId, DateTime? value, PuzzleUser author = null)
        {
            // If no player id is given, it implies that this is an unlock for all players
            if (!playerId.HasValue)
            {
                SinglePlayerPuzzleUnlockState unlockState = (await SinglePlayerPuzzleUnlockStateHelper.GetFullReadOnlyQuery(context, eventObj, puzzleId).ToListAsync()).Single();
                // Only allow unlock time to be modified if we were relocking it (setting it to null) or unlocking it for the first time 
                if (value == null || unlockState.UnlockedTime == null)
                {
                    unlockState.UnlockedTime = value;
                }
            }

            IQueryable<SinglePlayerPuzzleStatePerPlayer> statesQ = SinglePlayerPuzzleStateHelper.GetFullReadWriteQuery(context, eventObj, puzzleId, playerId, author);
            List<SinglePlayerPuzzleStatePerPlayer> states = await statesQ.ToListAsync();
            if (playerId.HasValue && puzzleId.HasValue && states.Count < 1)
            {
                context.SinglePlayerPuzzleStatePerPlayer.Add(new SinglePlayerPuzzleStatePerPlayer
                {
                    PuzzleID = puzzleId.Value,
                    PlayerID = playerId.Value,
                    UnlockedTime = value
                });
            }
            else
            {
                for (int i = 0; i < states.Count; i++)
                {
                    // Only allow unlock time to be modified if we were relocking it (setting it to null) or unlocking it for the first time 
                    if (value == null || states[i].UnlockedTime == null)
                    {
                        states[i].UnlockedTime = value;
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
