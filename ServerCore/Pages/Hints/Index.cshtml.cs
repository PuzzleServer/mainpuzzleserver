using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Hint> Hints { get;set; }

        public int PuzzleId { get; set; }

        public string PuzzleName { get; set; }

        public async Task OnGetAsync(int puzzleId)
        {
            PuzzleId = puzzleId;
            Hints = await _context.Hints.Where(hint => hint.Puzzle.ID == puzzleId).OrderBy(hint => hint.DisplayOrder).ThenBy(hint => hint.Description).ToListAsync();
            PuzzleName = await _context.Puzzles.Where(puzzle => puzzle.ID == puzzleId).Select(puzzle => puzzle.Name).FirstOrDefaultAsync();
        }
    }
}
