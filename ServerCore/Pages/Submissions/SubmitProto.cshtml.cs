using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Submissions
{
    [Authorize(Policy = "PlayerCanSeePuzzle")]
    public class SubmitProtoModel : EventSpecificPageModel
    {
        public SubmitProtoModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public Puzzle Puzzle { get; set; }

        public ContentFile PuzzleFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            await SetupContext(puzzleId);

            return Page();
        }

        private async Task SetupContext(int puzzleId)
        {
            Puzzle = await _context.Puzzles.Where(
                (p) => p.ID == puzzleId).FirstOrDefaultAsync();

            PuzzleFile = await _context.ContentFiles.Where(
                (f) => (f.Event == Event) && (f.Puzzle.ID == puzzleId) && (f.FileType == ContentFileType.Puzzle)
                ).FirstOrDefaultAsync();
        }
    }
}
