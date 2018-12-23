using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
{
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Hint> Hints { get;set; }

        public int PuzzleID { get; set; }

        public string PuzzleName { get; set; }

        public async Task OnGetAsync(int puzzleID)
        {
            PuzzleID = puzzleID;
            Hints = await _context.Hints.Where(hint => hint.Puzzle.ID == puzzleID).OrderBy(hint => hint.DisplayOrder).ThenBy(hint => hint.Description).ToListAsync();
            PuzzleName = await _context.Puzzles.Where(puzzle => puzzle.ID == puzzleID).Select(puzzle => puzzle.Name).FirstOrDefaultAsync();
        }
    }
}
