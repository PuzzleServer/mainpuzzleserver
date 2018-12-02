using System.Collections.Generic;
using System.Linq;
using ServerCore.DataModel;

namespace ServerCore
{
    public class AuthorDashboardHelper
    {
        /// <summary>
        /// Returns a list of the author's puzzles for the given event
        /// </summary>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public List<Puzzle> GetPuzzlesForAuthorAndEvent(PuzzleServerContext dbContext, PuzzleUser user, Event thisEvent)
        {
            return dbContext.PuzzleAuthors.Where(pa => pa.Author.ID == user.ID && pa.Puzzle.Event.ID == thisEvent.ID).Select(pa => pa.Puzzle).ToList();
        }
    }
}
