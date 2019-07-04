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
                .Where(tm => tm.Team.Event == Event)
                .Select(tm => tm.Member.Email)
                .ToListAsync();

            if (MailSubject?.Length > 0)
            {
                MailHelper.Singleton.SendPlaintextBcc(addresses, MailSubject, MailBody);
            }
            else
            {
                var mailtoUrl = "mailto:";
                // to: Add the proper support agent as recipient
                mailtoUrl += Event?.ContactEmail ?? "puzzhunt@microsoft.com";
                // cc: Cc the team's email address because what if this email account is a random personal one?
                if (Team.PrimaryContactEmail != null)
                {
                    mailtoUrl += "?cc=" + Uri.EscapeDataString(Team.PrimaryContactEmail);
                }

                // subject: Make this be about this puzzle
                mailtoUrl += "&subject=" + Uri.EscapeDataString("[" + Event.Name + "]");
                // subject: Make this be from this team
                mailtoUrl += Uri.EscapeDataString(" [" + Team.Name + "]");

                // request out of email mode
                // subject: Add the Email Mode signifier
                mailtoUrl += Uri.EscapeDataString(" [⚠ Team Disqualified ⚠]");
                // body: Invite solver to give details
                mailtoUrl += "&body=" + Uri.EscapeDataString(
                    "Use this email to fully explain your thought process so " +
                    "we know why you would like your team disqualification to " +
                    "be undone! 😇" + Environment.NewLine + Environment.NewLine);

                var DefaultMailBody =
                    "Your team has been disqualified from the event. You are " +
                    "welcome to continue solving puzzles for fun, but your " +
                    "team will no longer appear in the final standings and is " +
                    "not eligible for any awards or prizes that the event may " +
                    "offer. If you have any questions, please contact " +
                    "<a href=" + mailtoUrl + ">.";

                var DefaultMailSubject = "[" + Event.Name + "]" +
                    "[" + Team.Name + "] Disqualified from event";

                MailHelper.Singleton.SendPlaintextBcc(
                    addresses,
                    DefaultMailSubject,
                    DefaultMailBody);

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