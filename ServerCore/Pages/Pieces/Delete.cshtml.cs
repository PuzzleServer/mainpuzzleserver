using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Pieces
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class DeleteModel : EventSpecificPageModel
    {
        public DeleteModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public Piece Piece { get; set; }
        
        public int PuzzleId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Piece = await _context.Pieces.FirstOrDefaultAsync(m => m.ID == id);

            if (Piece == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Piece = await _context.Pieces.FindAsync(id);

            if (Piece != null)
            {
                _context.Pieces.Remove(Piece);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
