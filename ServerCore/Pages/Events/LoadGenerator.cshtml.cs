using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsEventAdmin")]
    public class LoadGeneratorModel : EventSpecificPageModel
    {
        [BindProperty]
        public int NotifyTeamId { get; set; }

        [BindProperty]
        public int NotifyCount { get; set; }

        [BindProperty]
        public int NotifySpacing { get; set; }

        public List<Team> Teams { get; set; }

        private IHubContext<ServerMessageHub> messageHub;

        public LoadGeneratorModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager, IHubContext<ServerMessageHub> messageHub) : base(serverContext, manager)
        {
            this.messageHub = messageHub;
        }

        public async Task<IActionResult> OnGetAsync(int teamId)
        {
            await SetupAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Team team = await _context.Teams.FindAsync(NotifyTeamId);

            for (int i = 0; i < NotifyCount; i++)
            {
                await this.messageHub.SendNotification(team, $"Notification {i + 1} of {NotifyCount}", "Consider yourself notified.");
                if (NotifySpacing > 0)
                {
                    await Task.Delay(NotifySpacing);
                }
            }

            await SetupAsync();
            return Page();
        }

        private async Task SetupAsync()
        {
            Teams = await _context.Teams.Where(t => t.EventID == Event.ID).OrderBy(t => t.Name).ToListAsync();
        }
    }
}
