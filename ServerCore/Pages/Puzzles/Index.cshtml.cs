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

        public class PuzzleView
        {
            public Puzzle Puzzle { get; set; }
            public ContentFile Content { get; set; }
        }

        public List<PuzzleView> Puzzles { get; set; }

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

            Puzzles = await (from Puzzle p in query
                             join ContentFile joinFile in _context.ContentFiles.Where((j) => j.Event == Event && j.FileType == ContentFileType.Puzzle) on p equals joinFile.Puzzle into fileJoin
                             from ContentFile file in fileJoin.DefaultIfEmpty()
                             orderby p.Group, p.OrderInGroup, p.Name
                             select new PuzzleView { Puzzle = p, Content = file }).ToListAsync();

            return Page();
        }
    }
}
