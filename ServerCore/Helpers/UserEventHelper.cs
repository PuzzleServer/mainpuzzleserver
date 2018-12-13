using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        public IQueryable<Puzzle> GetPuzzlesForAuthorAndEvent(PuzzleServerContext dbContext, Event thisEvent, PuzzleUser user)
        {
            return dbContext.PuzzleAuthors.Where(pa => pa.Author.ID == user.ID && pa.Puzzle.Event.ID == thisEvent.ID).Select(pa => pa.Puzzle);
        }

        /// <summary>
        /// Returns whether the user is an author of this puzzle
        /// </summary>
        /// <param name="puzzle">The puzzle that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public Task<bool> IsAuthorOfPuzzle(PuzzleServerContext dbContext, Puzzle puzzle, PuzzleUser user)
        {
            return dbContext.PuzzleAuthors.Where(pa => pa.Author.ID == user.ID && pa.Puzzle.ID == puzzle.ID).AnyAsync();
        }

        /// <summary>
        /// Returns the the team for the given player
        /// </summary>
        /// <param name="dbContext">Current PuzzleServerContext</param>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="user">The user being checked</param>
        /// <returns>The user's team for this event</returns>
        public Task<Team> GetTeamForPlayer(PuzzleServerContext dbContext, Event thisEvent, PuzzleUser user)
        {
            return dbContext.TeamMembers.Where(t => t.Member.ID == user.ID && t.Team.Event.ID == thisEvent.ID).Select(t => t.Team).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns the team for the currently signed in player
        /// </summary>
        /// <param name="puzzlerServerContext">Current PuzzleServerContext</param>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="user">The claim for the user being checked</param>
        /// <param name="userManager">The UserManager for the current context</param>
        /// <returns>The user's team for this event</returns>
        public Task<Team> GetTeamForCurrentPlayer(PuzzleServerContext puzzleServerContext, Event thisEvent, ClaimsPrincipal user, UserManager<IdentityUser> userManager)
        {
            PuzzleUser pUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleServerContext, user, userManager);
            return GetTeamForPlayer(puzzleServerContext, thisEvent, pUser);
        }
    }
}
