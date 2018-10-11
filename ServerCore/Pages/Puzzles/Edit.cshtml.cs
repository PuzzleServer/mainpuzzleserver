using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class EditModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public EditModel(PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Puzzle Puzzle { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Puzzle = await _context.Puzzles.Where(m => m.ID == id).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Puzzle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PuzzleExists(Puzzle.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PuzzleExists(int id)
        {
            return _context.Puzzles.Any(e => e.ID == id);
        }
    }
}
