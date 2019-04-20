using System;
using System.Collections.Concurrent;
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

        private static ConcurrentDictionary<string, PuzzleUser> puzzleUserCache = new ConcurrentDictionary<string, PuzzleUser>();
        private static DateTime expiryTime = DateTime.MinValue;
        private static TimeSpan expiryWindow = TimeSpan.FromMinutes(1);

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

            PuzzleUser result;

            DateTime now = DateTime.UtcNow;
            if (now > expiryTime)
            {
                puzzleUserCache.Clear();
                expiryTime = now + expiryWindow;
            }
            else
            {
                if (puzzleUserCache.TryGetValue(userId, out result))
                {
                    return result;
                }
            }

            result = await dbContext.PuzzleUsers.Where(u => u.IdentityUserId == userId).FirstOrDefaultAsync();
            puzzleUserCache[userId] = result;
            return result;
        }

        /// <summary>
        /// Returns whether or not a user is an author for the given event
        /// </summary>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public Task<bool> IsAuthorForEvent(PuzzleServerContext dbContext, Event thisEvent)
        {
            return thisEvent.IsAuthorInEvent(dbContext, this);
        }

        /// <summary>
        /// Returns whether or not a user is an admin for the given event
        /// </summary>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public Task<bool> IsAdminForEvent(PuzzleServerContext dbContext, Event thisEvent)
        {
            return thisEvent.IsAdminInEvent(dbContext, this);
        }


        /// <summary>
        /// Returns whether or not a user is a player in the given event
        /// </summary>
        /// <param name="thisEvent">The event that's being checked</param>
        /// <param name="puzzleServerContext">Current PuzzleServerContext</param>
        public Task<bool> IsPlayerInEvent(PuzzleServerContext dbContext, Event thisEvent)
        {
            return thisEvent.IsPlayerInEvent(dbContext, this);
        }
    }
}
