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

        [PersonalData]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [PersonalData]
        [MaxLength(20)]
        public string EmployeeAlias { get; set; }

        [PersonalData]
        [Required]
        [MaxLength(50)]
        public string Email { get; set; }
        public bool IsGlobalAdmin { get; set; }

        [PersonalData]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        [PersonalData]
        [MaxLength(20)]
        public string TShirtSize { get; set; }
        public bool VisibleToOthers { get; set; }

        /// <summary>
        /// True if the user is or has been an admin or author in any event.
        /// Saves querying whether a regular player is an admin or author of a particular event.
        /// </summary>
        public bool MayBeAdminOrAuthor { get; set; }

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
        public static async Task<PuzzleUser> GetPuzzleUserForCurrentUser(PuzzleServerContext dbContext, ClaimsPrincipal user, UserManager<IdentityUser> userManager)
        {
            if (userManager == null || dbContext == null)
            {
                //Default PageModel constructor used - cannot get current user.
                return new PuzzleUser { Name = String.Empty };
            }

            if (user == null)
            {
                return null;
            }

            string userId = userManager.GetUserId(user);
            return await dbContext.PuzzleUsers.Where(u => u.IdentityUserId == userId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns whether or not a user is an author for the given event
        /// </summary>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public async Task<bool> IsAuthorForEvent(PuzzleServerContext puzzleServerContext, Event thisEvent)
        {
            if (!MayBeAdminOrAuthor)
            {
                return false;
            }

            return await puzzleServerContext.EventAuthors.Where(a => a.Author.ID == ID && a.Event.ID == thisEvent.ID).AnyAsync();
        }

        /// <summary>
        /// Returns whether or not a user is an admin for the given event
        /// </summary>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public async Task<bool> IsAdminForEvent(PuzzleServerContext dbContext, Event thisEvent)
        {
            if (!MayBeAdminOrAuthor)
            {
                return false;
            }

            return await dbContext.EventAdmins.Where(a => a.Admin.ID == ID && a.Event.ID == thisEvent.ID).AnyAsync();
        }


        /// <summary>
        /// Returns whether or not a user has joined a team for the given event
        /// </summary>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public async Task<bool> IsPlayerOnTeam(PuzzleServerContext dbContext, Event thisEvent)
        {
            return await dbContext.TeamMembers.Where(tm => tm.Member.ID == ID && tm.Team.Event.ID == thisEvent.ID).AnyAsync();
        }

        /// <summary>
        /// Returns whether or not a user has completed the registration form for the given event
        /// </summary>
        /// <param name="dbContext">Current PuzzleServerContext</param>
        /// <param name="thisEvent">The event that's being checked</param>
        public async Task<bool> IsRegisteredForEvent(PuzzleServerContext dbContext, Event thisEvent)
        {
            return await dbContext.PlayerInEvent.Where(p => p.PlayerId == ID && p.EventId == thisEvent.ID).AnyAsync();
        }
    }
}
