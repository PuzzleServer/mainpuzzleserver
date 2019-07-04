using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{

    [Authorize("IsEventAdmin")]
    public class DisqualifyModel : EventSpecificPageModel
    {

        public DisqualifyModel(
            PuzzleServerContext serverContext, 
            UserManager<IdentityUser> userManager)
            : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public Team Team { get; set; }

        [BindProperty]
        public string MailSubject { get; set; }

        [BindProperty]
        public string MailBody { get; set; }

        public async Task<IActionResult> OnGetAsync(int teamId)
        {
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);

            if (Team == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDisqualifyAsync(int teamId)
        {
            Team = await _context.Teams.FindAsync(teamId);

            if (Team != null)
            {
                await TeamHelper.SetTeamQualificationAsync(_context, Team, true);
            }

            if (EventRole == EventRole.admin)
            {
                return RedirectToPage("./Index");
            }
            else
            {
                return RedirectToPage("./List");
            }
        }

        public async Task<IActionResult> OnPostRequalifyAsync(int teamId)
        {
            Team = await _context.Teams.FindAsync(teamId);

            if (Team != null)
            {
                await TeamHelper.SetTeamQualificationAsync(_context, Team, false);
            }

            if (MailSubject.Length > 0)
            {
                IEnumerable<string> addresses = Enumerable.Empty<string>();
                addresses = await _context.TeamMembers
                    .Where(tm => tm.Team.Event == Event)
                    .Select(tm => tm.Member.Email)
                    .ToListAsync();

                MailHelper.Singleton.SendPlaintextBcc(addresses, MailSubject, MailBody);
            }

            if (EventRole == EventRole.admin)
            {
                return RedirectToPage("./Index");
            }
            else
            {
                return RedirectToPage("./List");
            }
        }
    }
}