using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    public class DeleteModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public DeleteModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Response PuzzleResponse { get; set; }
        
        public int PuzzleId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, int puzzleId)
        {
            PuzzleResponse = await _context.Responses.FirstOrDefaultAsync(m => m.ID == id);
            PuzzleId = puzzleId;

            if (Response == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, int puzzleId)
        {
            PuzzleResponse = await _context.Responses.FindAsync(id);

            if (Response != null)
            {
                _context.Responses.Remove(PuzzleResponse);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index", new { puzzleId = puzzleId });
        }
    }
}
