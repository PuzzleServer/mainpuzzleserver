using ServerCore.DataModel;
using System.Linq;
using System;

namespace ServerCore
{
    public class SinglePlayerPuzzleUnlockStateHelper
    {
        /// <summary>
        /// Get a read-only query of single player puzzle state. You won't be able to write to these values, but the query will be resilient to state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzleId">The puzzle id; if null, get all puzzles in the event.</param>
        /// <returns>A query of SinglePlayerPuzzleUnlockState objects that can be sorted and instantiated, but you can't edit the results.</returns>
        public static IQueryable<SinglePlayerPuzzleUnlockState> GetFullReadOnlyQuery(PuzzleServerContext context, Event eventObj, int? puzzleId)
        {
            if (context == null)
            {
                throw new ArgumentNullException("Context required.");
            }

            if (eventObj == null)
            {
                throw new ArgumentNullException("Event required.");
            }

            if (puzzleId.HasValue)
            {
                return context.SinglePlayerPuzzleUnlockStates.Where(state => state.PuzzleID == puzzleId);
            }

            return context.SinglePlayerPuzzleUnlockStates.Where(state => state.Puzzle.Event == eventObj);
        }
    }
}