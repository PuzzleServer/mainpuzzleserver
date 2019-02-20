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
        public Puzzle Puzzle { get; set; }

        public CreateModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IActionResult OnGet()
        {
            // Populate default fields
            Puzzle = new Puzzle();
            Puzzle.IsPuzzle = true;
            Puzzle.SolveValue = 10;
            Puzzle.OrderInGroup = 0;
            Puzzle.MinPrerequisiteCount = 0;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Puzzle.Event");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Puzzle.Event = Event;

            _context.Puzzles.Add(Puzzle);
            _context.PuzzleAuthors.Add(new PuzzleAuthors() { Puzzle = Puzzle, Author = LoggedInUser });

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}