using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Pieces
{
    [Authorize(Policy = "IsAuthorOfPuzzle")]
    public class EditModel : EventSpecificPageModel
    {
        public EditModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public Piece Piece { get; set; }

        public int PuzzleId { get; set; }

        public Puzzle Puzzle { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Piece = await _context.Pieces.FirstOrDefaultAsync(m => m.ID == id);

            if (Piece == null)
            {
                return NotFound();
            }

            PuzzleId = Piece.PuzzleID;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Piece).State = EntityState.Modified;
            PuzzleId = Piece.PuzzleID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PieceExists(Piece.ID))
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

        private bool PieceExists(int id)
        {
            return _context.Pieces.Any(e => e.ID == id);
        }
    }
}
