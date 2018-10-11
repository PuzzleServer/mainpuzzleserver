using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    public class DeleteModel : EventSpecificPageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public DeleteModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Response PuzzleResponse { get; set; }
        
        public int PuzzleId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PuzzleResponse = await _context.Responses.FirstOrDefaultAsync(m => m.ID == id);

            if (PuzzleResponse == null)
            {
                return NotFound();
            }

            PuzzleId = PuzzleResponse.Puzzle.ID;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            PuzzleResponse = await _context.Responses.FindAsync(id);
            PuzzleId = PuzzleResponse.PuzzleID;

            if (PuzzleResponse != null)
            {
                _context.Responses.Remove(PuzzleResponse);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index", new { puzzleId = PuzzleId });
        }
    }
}
