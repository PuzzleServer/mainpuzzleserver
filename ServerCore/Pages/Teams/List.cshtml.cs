using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;

namespace ServerCore.Pages.Teams
{
    /// <summary>
    /// Player-facing list of all teams
    /// </summary>
    [AllowAnonymous]
    public class ListModel : TeamListBase
    {
        public ListModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        /// <summary>
        /// True if the current user is on a team
        /// </summary>
        public bool UserOnTeam { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if (await LoggedInUser.IsPlayerInEvent(_context, Event))
            {
                UserOnTeam = true;
            }
            else
            {
                UserOnTeam = false;
            }

            await LoadTeamDataAsync();

            return Page();
        }
    }
}
