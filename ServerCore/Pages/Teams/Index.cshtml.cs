using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Team> Teams { get;set; }
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

            Teams = await _context.Teams.Where(team => team.Event == Event).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetJoinTeamAsync(int teamId)
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if (await LoggedInUser.IsPlayerInEvent(_context, Event))
            {
                return Page();
            }

            // add an application and delete old ones here
            return Page();
        }
    }
}
