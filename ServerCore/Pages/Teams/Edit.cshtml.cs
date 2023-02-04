using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [Authorize(Policy = "IsEventAdminOrPlayerOnTeam")]
    public class EditModel : EventSpecificPageModel
    {
        public EditModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public Team Team { get; set; }

        public async Task<IActionResult> OnGetAsync(int teamId)
        {
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);

            if (Team == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Team.PrimaryContactEmail))
            {
                ModelState.AddModelError("Team.PrimaryContactEmail", "An email is required.");
            }
            else if (!MailHelper.IsValidEmail(Team.PrimaryContactEmail))
            {
                ModelState.AddModelError("Team.PrimaryContactEmail", "This email address is not valid.");
            }

            ModelState.Remove("Team.Event");
            if (!ModelState.IsValid)
            {
                return Page();
            }
            Team.EventID = Event.ID;

            Team existingTeam = await (from team in _context.Teams
                                where team.ID == Team.ID
                                select team).SingleOrDefaultAsync();
            if (existingTeam == null)
            {
                return NotFound();
            }

            // If the name cannot be changed, ignore input
            // Note: It should generally never reach this point because the form itself should not allow this to change.
            if (!this.CanChangeTeamName())
            {
                Team.Name = existingTeam.Name;
            }

            if (Team.Name.Length > 50)
            {
                ModelState.AddModelError("Team.Name", "Name too long. Character limit: 50");
                return Page();
            }

            if (Team.Name != existingTeam.Name && await TeamHelper.IsTeamNameTakenAsync(_context, Event, Team.Name))
            {
                ModelState.AddModelError("Team.Name", "Another team has this name.");
                return Page();
            }

            // keep the room unchanged if an intern event, since interns can't edit their rooms
            if (Event.IsInternEvent && EventRole != EventRole.admin)
            {
                Team.CustomRoom = existingTeam.CustomRoom;
            }

            // Avoid letting the team tamper with their hint coin count
            Team.HintCoinCount = existingTeam.HintCoinCount;
            _context.Entry(existingTeam).State = EntityState.Detached;
            _context.Attach(Team).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamExists(Team.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Details", new { eventId = Event.ID, eventRole = EventRole, teamId = Team.ID });
        }

        public bool CanChangeTeamName()
        {
            return EventRole == EventRole.admin || Event.CanChangeTeamName;
        }

        private bool TeamExists(int teamId)
        {
            return _context.Teams.Any(e => e.ID == teamId);
        }
    }
}
