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
    public class ListModel : TeamListBase
    {
        public ListModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        /// <summary>
        /// True if the current user is on a team
        /// </summary>
        public bool UserOnTeam { get; set; }

        public Team AppliedTeam { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if (Event.HasIndividualRegistration && !(await LoggedInUser.IsRegisteredForEvent(_context, Event)))
            {
                return RedirectToPage("/Player/Create");
            }

            TeamMembers playerTeam = await (from member in _context.TeamMembers
                                            where member.Member == LoggedInUser &&
                                            member.Team.Event == Event
                                            select member).FirstOrDefaultAsync();
            if (playerTeam != null)
            {
                return RedirectToPage("./Details", new { teamId = playerTeam.Team.ID });
            }

            var applications = from application in _context.TeamApplications
                               where application.Player == LoggedInUser && application.Team.Event == Event
                               select application.Team;
            AppliedTeam = await applications.FirstOrDefaultAsync();

            await LoadTeamDataAsync();

            return Page();
        }
    }
}
