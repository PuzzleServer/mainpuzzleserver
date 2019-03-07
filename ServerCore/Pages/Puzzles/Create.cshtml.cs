using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class CreateModel : EventSpecificPageModel
    {
        [BindProperty]
        public MiniPuzzle Puzzle { get; set; }

        public CreateModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IActionResult OnGet()
        {
            // Populate default fields
            Puzzle = new MiniPuzzle();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Puzzle p = new Puzzle();

            p.Event = Event;
            p.Name = Puzzle.Name;
            p.Group = Puzzle.Group;
            p.OrderInGroup = Puzzle.OrderInGroup;

            switch (Puzzle.Type)
            {
                case PuzzleType.FinalMetaPuzzle:
                    p.IsFinalPuzzle = true;
                    p.IsMetaPuzzle = true;
                    p.IsPuzzle = true;
                    p.SolveValue = 1000;
                    p.MinPrerequisiteCount = 1;
                    break;
                case PuzzleType.MetaPuzzle:
                    p.IsMetaPuzzle = true;
                    p.IsPuzzle = true;
                    p.SolveValue = 50;
                    p.MinPrerequisiteCount = 1;
                    break;
                case PuzzleType.CheatCode:
                    p.IsPuzzle = true;
                    p.IsCheatCode = true;
                    p.SolveValue = -1;
                    p.MinPrerequisiteCount = 1;
                    break;
                case PuzzleType.NormalPuzzle:
                    p.IsPuzzle = true;
                    p.SolveValue = 10;
                    p.MinPrerequisiteCount = 1;
                    break;
                case PuzzleType.InvisiblePuzzle:
                    p.MinPrerequisiteCount = 1;
                    break;
                case PuzzleType.StartPuzzle:
                    break;
            }

            _context.Puzzles.Add(p);
            _context.PuzzleAuthors.Add(new PuzzleAuthors() { Puzzle = p, Author = LoggedInUser });

            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { puzzleId = p.ID });
        }

        public class MiniPuzzle
        {
            [Required]
            public string Name { get; set; }
            public string Group { get; set; }
            public int OrderInGroup { get; set; }
            public PuzzleType Type { get; set; }
        }

        public enum PuzzleType
        {
            NormalPuzzle,
            MetaPuzzle,
            FinalMetaPuzzle,
            CheatCode,
            StartPuzzle,
            InvisiblePuzzle
        }
    }
}