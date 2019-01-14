using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class CreateModel : EventSpecificPageModel
    {
        public CreateModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public Response PuzzleResponse { get; set; }

        public int PuzzleId { get; set; }

        public IActionResult OnGet(int puzzleId)
        {
            PuzzleId = puzzleId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            PuzzleResponse.PuzzleID = puzzleId;

            _context.Responses.Add(PuzzleResponse);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { puzzleid = puzzleId });
        }
    }
}