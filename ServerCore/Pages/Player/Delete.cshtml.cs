using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Player
{
    // An empty Authorize attribute requires that the person is signed in but sets no other requirements
    [Authorize]
    public class DeleteModel : EventSpecificPageModel
    {
        public DeleteModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        [BindProperty]
        public PlayerInEvent PlayerInEvent { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlayerInEvent = await _context.PlayerInEvent
                .Include(p => p.Event)
                .Include(p => p.Player).FirstOrDefaultAsync(m => m.ID == id);

            if (PlayerInEvent == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlayerInEvent = await _context.PlayerInEvent.FindAsync(id);

            if (PlayerInEvent != null)
            {
                _context.PlayerInEvent.Remove(PlayerInEvent);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("../Index");
        }
    }
}
