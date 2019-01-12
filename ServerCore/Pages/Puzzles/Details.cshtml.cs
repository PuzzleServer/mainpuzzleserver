using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class DetailsModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public DetailsModel(PuzzleServerContext context)
        {
            _context = context;
        }

        public Puzzle Puzzle { get; set; }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
