using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    public class EditModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public EditModel(ServerCore.Models.PuzzleServerContext context)
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

            PuzzleId = PuzzleResponse.PuzzleID;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(PuzzleResponse).State = EntityState.Modified;
            PuzzleId = PuzzleResponse.PuzzleID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResponseExists(PuzzleResponse.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index", new { puzzleId = PuzzleId });
        }

        private bool ResponseExists(int id)
        {
            return _context.Responses.Any(e => e.ID == id);
        }
    }
}
