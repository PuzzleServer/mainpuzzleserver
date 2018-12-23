using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Puzzle> Puzzles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (EventRole != EventRole.admin && EventRole != EventRole.author)
            {
                return NotFound();
            }

            if (EventRole == EventRole.admin)
            {
                Puzzles = await _context.Puzzles.Where(p => p.Event == Event).ToListAsync();
            }
            else
            {
                Puzzles = await UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser).ToListAsync();
            }

            return Page();
        }
    }
}
