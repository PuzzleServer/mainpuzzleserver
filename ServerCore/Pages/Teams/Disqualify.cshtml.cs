using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

            IEnumerable<string> addresses = Enumerable.Empty<string>();
            addresses = await _context.TeamMembers
                .Where(tm => (tm.Team.Event == Event && tm.Team.ID == Team.ID))
                .Select(tm => tm.Member.Email)
                .ToListAsync();

            if (MailSubject?.Length > 0)
            {
                MailHelper.Singleton.SendPlaintextBcc(addresses, MailSubject, MailBody);
            }
            else
            {
                var DefaultMailBody =
                    "Your team has been disqualified from the event. You are " +
                    "welcome to continue solving puzzles for fun, but your " +
                    "team will no longer appear in the final standings and is " +
                    "not eligible for any awards or prizes that the event may " +
                    "offer. If you have any questions, please contact " +
                    (Event?.ContactEmail ?? "puzzhunt@microsoft.com");

                var DefaultMailSubject = "[" + Event.Name + "]" +
                    "[" + Team.Name + "] Disqualified from event";

                MailHelper.Singleton.SendPlaintextBcc(
                    addresses,
                    DefaultMailSubject,
                    DefaultMailBody);

            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostRequalifyAsync(int teamId)
        {
            Team = await _context.Teams.FindAsync(teamId);

            if (Team != null)
            {
                await TeamHelper.SetTeamQualificationAsync(_context, Team, false);
            }

            return RedirectToPage("./Index");
        }
    }
}