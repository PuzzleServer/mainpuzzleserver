using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Helpers
{
    /// <summary>
    /// Includes methods that connect a PuzzleUser with another part of the data model
    /// </summary>
    public class UserEventHelper
    {
        /// <summary>
        /// Returns a list of the author's puzzles for the given event
        /// </summary>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public List<Puzzle> GetPuzzlesForAuthorAndEvent(PuzzleServerContext dbContext, Event thisEvent, PuzzleUser user)
        {
            return dbContext.PuzzleAuthors.Where(pa => pa.Author.ID == user.ID && pa.Puzzle.Event.ID == thisEvent.ID).Select(pa => pa.Puzzle).ToList();
        }

        /// <summary>
        /// Returns the the team for the given player
        /// </summary>
        /// <param name="dbContext">Current PuzzleServerContext</param>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="user">The user being checked</param>
        /// <returns>The user's team for this event</returns>
        public Team GetTeamForPlayer(PuzzleServerContext dbContext, Event thisEvent, PuzzleUser user)
        {
            return dbContext.TeamMembers.Where(t => t.Member.ID == user.ID && t.Team.Event.ID == thisEvent.ID).Select(t => t.Team).First();
        }

        /// <summary>
        /// Returns the team for the currently signed in player
        /// </summary>
        /// <param name="puzzlerServerContext">Current PuzzleServerContext</param>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="user">The claim for the user being checked</param>
        /// <param name="userManager">The UserManager for the current context</param>
        /// <returns>The user's team for this event</returns>
        public Team GetTeamForCurrentPlayer(PuzzleServerContext puzzleServerContext, Event thisEvent, ClaimsPrincipal user, UserManager<IdentityUser> userManager)
        {
            PuzzleUser pUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleServerContext, user, userManager);
            return GetTeamForPlayer(puzzleServerContext, thisEvent, pUser);
        }
    }
}
