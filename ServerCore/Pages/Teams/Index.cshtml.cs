using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Teams
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class IndexModel : TeamListBase
    {
        [BindProperty]
        public int SmallTeamThreshold { get; set; } = 6;

        private IHubContext<ServerMessageHub> messageHub;

        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager, IHubContext<ServerMessageHub> messageHub) : base(serverContext, userManager)
        {
            this.messageHub = messageHub;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadTeamDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetShowTeamAnnouncementAsync(int teamId, bool value)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);

            if (team == null)
            {
                return NotFound();
            }

            if (team.ShowTeamAnnouncement != value)
            {
                team.ShowTeamAnnouncement = value;

                IEnumerable<string> addresses = Enumerable.Empty<string>();
                addresses = await _context.TeamMembers
                    .Where(tm => (tm.Team.Event == Event && tm.Team.ID == teamId))
                    .Select(tm => tm.Member.Email)
                    .ToListAsync();

                var MailBody =
                    (value ? Event.TeamAnnouncement : "This announcement has been revoked for your team.") +
                    " If you have any questions, please contact " +
                    (Event?.ContactEmail ?? "puzzhunt@microsoft.com");

                var MailSubject = "[" + Event.Name + "]" +
                    "[" + team.Name + "] " + (value ? "" : "[REVOKED!!!] ") + Event.TeamAnnouncement;

                MailHelper.Singleton.SendPlaintextBcc(
                    addresses,
                    MailSubject,
                MailBody);

                await this.messageHub.SendNotification(team, $"Hey {team.Name}", (value ? "" : "[REVOKED!!!] ") + Event.TeamAnnouncement, isCritical: true);

                await this._context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
