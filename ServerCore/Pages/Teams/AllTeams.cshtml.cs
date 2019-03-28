using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Teams
{
    /// <summary>
    /// Player-facing list of all teams
    /// </summary>
    [AllowAnonymous]
    public class AllTeamsModel : TeamListBase
    {
        public bool PlayerNotOnTeam { get; set; }

        public AllTeamsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            PlayerNotOnTeam = -1 == await GetTeamId();

            await LoadTeamDataAsync();

            return Page();
        }
    }
}
