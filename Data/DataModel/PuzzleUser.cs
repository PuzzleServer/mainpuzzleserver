using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Custom user object that holds the puzzle user specific data (i.e. the data that's used by the puzzle system, not the data used for auth)
    /// </summary>
    [PersonalData]
    public class PuzzleUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Links the puzzle user to their authentication identity
        /// </summary>
        [ForeignKey("AspNetUsers.Id")]
        [Required]
        public string IdentityUserId { get; set; }

        public string Name { get; set; }
        public string EmployeeAlias { get; set; }
        public string Email { get; set; }
        public bool IsGlobalAdmin { get; set; }
        public string PhoneNumber { get; set; }
        public string TShirtSize { get; set; }
        public bool VisibleToOthers { get; set; }

        /// <summary>
        /// Returns the PuzzleUser for the given IdentityUser
        /// </summary>
        /// <param name="identityUserId">The string Id of an IdentityUser</param>
        /// <param name="dbContext">The current PuzzleServerContext</param>
        /// <returns>A PuzzleUser object that corresponds to the given IdentityUser</returns>
        public static async Task<PuzzleUser> GetPuzzleUser(string identityUserId, PuzzleServerContext dbContext)
        {
            return await dbContext.PuzzleUsers.Where(user => user.IdentityUserId == identityUserId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns the PuzzleUser for the currently signed in player
        /// </summary>
        /// <param name="puzzlerServerContext">Current PuzzleServerContext</param>
        /// <param name="user">The claim for the user being checked</param>
        /// <param name="userManager">The UserManager for the current context</param>
        /// <returns>The user's PuzzleUser object</returns>
        public static PuzzleUser GetPuzzleUserForCurrentUser(PuzzleServerContext puzzleServerContext, ClaimsPrincipal user, UserManager<IdentityUser> userManager)
        {
            if (userManager == null || puzzleServerContext == null)
            {
                //Default PageModel constructor used - cannot get current user.
                return new PuzzleUser { Name = String.Empty };
            }

            if (user == null)
            {
                return new PuzzleUser { Name = String.Empty };
            }

            string userId = userManager.GetUserId(user);
            return puzzleServerContext.PuzzleUsers.Where(u => u.IdentityUserId == userId).FirstOrDefault();
        }

        /// <summary>
        /// Returns whether or not a user is an author for the given event
        /// </summary>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public bool IsAuthorForEvent(PuzzleServerContext puzzleServerContext, Event thisEvent)
        {
            return puzzleServerContext.EventAuthors.Where(a => a.Author.ID == ID && a.Event.ID == thisEvent.ID).Any();
        }

        /// <summary>
        /// Returns whether or not a user is an admin for the given event
        /// </summary>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public bool IsAdminForEvent(PuzzleServerContext dbContext, Event thisEvent)
        {
            return dbContext.EventAdmins.Where(a => a.Admin.ID == ID && a.Event.ID == thisEvent.ID).Any();
        }

        public bool IsPlayerForEvent(PuzzleServerContext dbContext, Event thisEvent)
        {
           // return dbContext.EventTeams.Where(t=> t.Team.)
        }
    }
}
