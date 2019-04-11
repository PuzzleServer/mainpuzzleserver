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
                return Forbid();
            }

            if (!Event.IsTeamMembershipChangeActive)
            {
                return NotFound("Team membership change is not currently allowed.");
            }

            TeamMembers playerTeam = await (from member in _context.TeamMembers
                                     where member.Member == LoggedInUser &&
                                     member.Team.Event == Event
                                     select member).FirstOrDefaultAsync();
            if (playerTeam != null)
            {
                return RedirectToPage("./Details", new { teamId = playerTeam.Team.ID });
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

            // TODO I am well aware that it would be far better to include a direct link to the details page,
            // but I cannot for the life of me figure out how to generate a URL for a Razor page.
            MailHelper.Singleton.SendPlaintextWithoutBcc(new string[] { Team.PrimaryContactEmail, LoggedInUser.Email },
                $"{LoggedInUser.Name} is applying to join {Team.Name}",
                $"To accept or reject this request, visit your team page.");

            return Page();
        }
    }
}