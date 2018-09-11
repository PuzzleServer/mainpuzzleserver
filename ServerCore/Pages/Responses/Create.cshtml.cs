using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Responses
{
    public class CreateModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public CreateModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Response PuzzleResponse { get; set; }

        public int PuzzleId { get; set; }

        public int EventId { get; set; }

        public IActionResult OnGet(int puzzleId, int eventId)
        {
            this.PuzzleId = puzzleId;
            this.EventId = eventId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            PuzzleResponse.Puzzle = await _context.Puzzles.SingleOrDefaultAsync(m => m.ID == puzzleId);

            _context.Responses.Add(PuzzleResponse);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { puzzleid = puzzleId, eventid = PuzzleResponse.Puzzle.Event.ID });
        }
    }
}