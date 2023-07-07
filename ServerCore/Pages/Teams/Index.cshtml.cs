using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Teams
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class IndexModel : TeamListBase
    {
        [BindProperty]
        public int SmallTeamThreshold { get; set; } = 6;

        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadTeamDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetShowTeamAnnouncementAsync(int teamId, bool value)
        {
            var team = await _context.Teams.FirstAsync(m => m.ID == teamId);

            if (team == null)
            {
                return NotFound();
            }

            team.ShowTeamAnnouncement = value;

            IEnumerable<string> addresses = Enumerable.Empty<string>();
            addresses = await _context.TeamMembers
                .Where(tm => (tm.Team.Event == Event && tm.Team.ID == teamId))
                .Select(tm => tm.Member.Email)
                .ToListAsync();

            var MailBody =
                (value ? "" : "This announcement has been revoked for your team. ") +
                "If you have any questions, please contact " +
                (Event?.ContactEmail ?? "puzzhunt@microsoft.com");

            var MailSubject = "[" + Event.Name + "]" +
                "[" + team.Name + "] " + (value ? "" : "[REVOKED!!!] ") + Event.TeamAnnouncement;

            MailHelper.Singleton.SendPlaintextBcc(
                addresses,
                MailSubject,
                MailBody);

            await this._context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
