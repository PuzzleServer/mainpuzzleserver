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
    /// <summary>
    /// Page for players to apply to join a team
    /// </summary>
    [AllowAnonymous] // AuthZ normally requires players to be on a team, but this page is for joining, so allow them
    public class ApplyModel : EventSpecificPageModel
    {
        public ApplyModel(PuzzleServerContext context, UserManager<IdentityUser> userManager)
            : base(context, userManager)
        {
        }

        public Team Team { get; set; }

        public async Task<IActionResult> OnGet(int teamID)
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if (EventRole != EventRole.play)
            {
                return NotFound();
            }

            if (!Event.IsTeamMembershipChangeActive)
            {
                return NotFound("Team membership change is not currently allowed.");
            }

            if (await (from member in _context.TeamMembers
                       where member.Member == LoggedInUser &&
                       member.Team.Event == Event
                       select member).AnyAsync())
            {
                return NotFound("You're already on a team!");
            }

            Team = await (from Team t in _context.Teams
                               where t.ID == teamID && t.Event == Event
                               select t).FirstOrDefaultAsync();

            if (Team == null)
            {
                return NotFound();
            }

            // Only handle one application at a time for a player to avoid spamming all teams
            IEnumerable<TeamApplication> oldApplications = from TeamApplication oldApplication in _context.TeamApplications
                                                           where oldApplication.Player == LoggedInUser && oldApplication.Team.Event == Event
                                                           select oldApplication;
            _context.TeamApplications.RemoveRange(oldApplications);

            TeamApplication application = new TeamApplication()
            {
                Team = Team,
                Player = LoggedInUser
            };

            _context.TeamApplications.Add(application);

            await _context.SaveChangesAsync();

            return Page();
        }
    }
}