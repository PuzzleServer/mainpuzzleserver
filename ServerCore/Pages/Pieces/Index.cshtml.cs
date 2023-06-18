using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Pieces
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Piece> Pieces { get;set; }

        public int PuzzleId { get; set; }

        public Puzzle Puzzle { get; set; }

        public async Task OnGetAsync(int puzzleId)
        {
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            Pieces = await _context.Pieces.Where((r) => r.Puzzle != null && r.Puzzle.ID == puzzleId).OrderBy(p => p.ProgressLevel).ThenBy(p => p.Contents).ToListAsync();
            PuzzleId = puzzleId;
        }
    }
}
