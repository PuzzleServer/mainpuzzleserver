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
    public class DetailsModel : EventSpecificPageModel
    {
        public DetailsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

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

            if(await LoggedInUser.IsAdminForEvent(_context, Event) || LoggedInUser.ID == PlayerInEvent.PlayerId)
            {
                return Page();
            }

            return Forbid();
        }
    }
}
