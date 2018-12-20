using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class DeleteModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public DeleteModel(PuzzleServerContext context)
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

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Puzzle = await _context.Puzzles.Where(m => m.ID == id).FirstOrDefaultAsync();

            if (Puzzle != null)
            {
                // Delete all files associated with this puzzle
                foreach (ContentFile content in Puzzle.Contents)
                {
                    await FileManager.DeleteBlobAsync(content.Url);
                }

                // Delete all Prerequisites where this puzzle depends on or is depended upon by another puzzle
                foreach (Prerequisites thisPrerequisite in _context.Prerequisites.Where((r) => r.Puzzle == Puzzle || r.Prerequisite == Puzzle))
                {
                    _context.Prerequisites.Remove(thisPrerequisite);
                }

                _context.Puzzles.Remove(Puzzle);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
