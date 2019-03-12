using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Puzzle> Puzzles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IQueryable<Puzzle> query;

            if (EventRole == EventRole.admin)
            {
                query = _context.Puzzles.Where(p => p.Event == Event);
            }
            else
            {
                query = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser);
            }

            Puzzles = await query.OrderBy((p) => p.Group).ThenBy((p) => p.OrderInGroup).ThenBy((p) => p.Name).ToListAsync();

            return Page();
        }
    }
}
