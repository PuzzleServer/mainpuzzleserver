using System;
using System.Collections.Generic;
using System.IO;
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
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class CreateBulkModel : EventSpecificPageModel
    {
        public CreateBulkModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public string Pieces { get; set; }

        public int PuzzleId { get; set; }

        public Puzzle Puzzle { get; set; }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            PuzzleId = puzzleId;
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            StringReader piecesReader = new StringReader(Pieces ?? string.Empty);

            var newPieces = new List<Piece>();

            while (true)
            {
                string pieceEncoded = piecesReader.ReadLine();
                if (pieceEncoded == null) {
                    break;
                }
                if (pieceEncoded.Length == 0) {
                    continue;
                }

                int tabIndex = pieceEncoded.IndexOf('\t');
                if (tabIndex < 0) {
                    ModelState.AddModelError("Piece", "Missing tab in line");
                    break;
                }

                string progressLevelString = pieceEncoded.Substring(0, tabIndex);
                var progressLevel = Convert.ToInt32(progressLevelString);
                string contents = pieceEncoded.Substring(tabIndex+1);

                Piece newPiece = new Piece() { PuzzleID = puzzleId, ProgressLevel = progressLevel, Contents = contents };
                newPieces.Add(newPiece);
            }

            if (!ModelState.IsValid)
            {
                return await OnGetAsync(puzzleId);
            }

            var currentPieces = await _context.Pieces.Where(p => p.PuzzleID == puzzleId).ToListAsync();
            _context.Pieces.RemoveRange(currentPieces);
            await _context.SaveChangesAsync();

            _context.Pieces.AddRange(newPieces);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { puzzleid = puzzleId });
        }
    }
}
