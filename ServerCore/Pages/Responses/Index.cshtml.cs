using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Response> Responses { get;set; }
        
        public int PuzzleId { get; set; }

        public Puzzle Puzzle { get; set; }

        public async Task OnGetAsync(int puzzleId)
        {
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            Responses = await _context.Responses.Where((r) => r.Puzzle != null && r.Puzzle.ID == puzzleId).OrderByDescending((r) => r.IsSolution).ThenBy((r) => r.ResponseText).ThenBy((r) => r.SubmittedText).ToListAsync();
            PuzzleId = puzzleId;
        }
    }
}
