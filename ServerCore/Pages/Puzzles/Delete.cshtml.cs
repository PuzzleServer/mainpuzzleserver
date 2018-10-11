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
                foreach (ContentFile content in Puzzle.Contents)
                {
                    await FileManager.DeleteBlobAsync(content.Url);
                }

                _context.Puzzles.Remove(Puzzle);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
