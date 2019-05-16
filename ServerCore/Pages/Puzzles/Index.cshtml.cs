using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public class PuzzleView
        {
            public Puzzle Puzzle { get; set; }
            public ContentFile Content { get; set; }
        }

        public List<PuzzleView> Puzzles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            List<Puzzle> puzzles = await PuzzleHelper.GetPuzzles(_context, Event, LoggedInUser, EventRole);
            Dictionary<int, ContentFile> puzzleFiles = await (from file in _context.ContentFiles
                                                              where file.Event == Event && file.FileType == ContentFileType.Puzzle
                                                              select file).ToDictionaryAsync(file => file.PuzzleID);

            Puzzles = new List<PuzzleView>();
            foreach (Puzzle puzzle in puzzles)
            {
                puzzleFiles.TryGetValue(puzzle.ID, out ContentFile content);
                Puzzles.Add(new PuzzleView()
                {
                    Puzzle = puzzle,
                    Content = content
                });
            }

            return Page();
        }
    }
}
