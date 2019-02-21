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

        public async Task<IActionResult> OnPostAsync(int teamId)
        {
            ModelState.Remove("Team.ID");
            ModelState.Remove("Team.Event");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Team.ID = teamId;
            Team.Event = Event;

            var teamToDetach = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            _context.Entry(teamToDetach).State = EntityState.Detached;

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

            if (EventRole == ModelBases.EventRole.play)
            {
                return RedirectToPage("./Details");
            }
            return RedirectToPage("./Index");
        }

        private bool TeamExists(int teamId)
        {
            return _context.Teams.Any(e => e.ID == teamId);
        }
    }
}
