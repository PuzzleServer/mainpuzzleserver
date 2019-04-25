using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    // Note: Mailer is restricted to admins because the number of messages we can send is a limited resource.
    // This prevents author abuse/ignorance.
    [Authorize(Policy = "IsEventAdmin")]
    public class MailerModel : EventSpecificPageModel
    {
        public enum MailGroup
        {
            Players,
            PrimaryContacts
        }

        [BindProperty]
        public int? TeamID { get; set; }

        public Team Team { get; set; }

        [BindProperty]
        public MailGroup Group { get; set; }

        [BindProperty]
        [Required]
        public string MailSubject { get; set; }

        [BindProperty]
        [Required]
        public string MailBody { get; set; }

        public MailerModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(MailGroup group, int? teamId)
        {
            Group = group;
            TeamID = teamId;

            if (teamId != null)
            {
                Team = await _context.Teams.FindAsync(teamId);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            IEnumerable<string> addresses = Enumerable.Empty<string>();

            switch(Group)
            {
                case MailGroup.Players:
                    if (TeamID == null)
                    {
                        addresses = await _context.TeamMembers.Where(tm => tm.Team.Event == Event).Select(tm => tm.Member.Email).ToListAsync();
                    }
                    else
                    {
                        addresses = await _context.TeamMembers.Where(tm => tm.Team.ID == TeamID).Select(tm => tm.Member.Email).ToListAsync();
                    }
                    break;
                case MailGroup.PrimaryContacts:
                    List<string> primaries;

                    if (TeamID == null)
                    {
                        primaries = await _context.Teams.Where(t => t.Event == Event).Select(t => t.PrimaryContactEmail).ToListAsync();
                    }
                    else
                    {
                        primaries = await _context.Teams.Where(t => t.ID == TeamID).Select(t => t.PrimaryContactEmail).ToListAsync();
                    }

                    List<string> primariesSplit = new List<string>();

                    foreach (string p in primaries)
                    {
                        if (p != null)
                        {
                            primariesSplit.AddRange(p.Split(',', ';'));
                        }
                    }

                    addresses = primariesSplit;
                    break;
            }

            MailHelper.Singleton.SendPlaintextBcc(addresses, MailSubject, MailBody);

            return RedirectToPage("./Players");
        }
    }
}