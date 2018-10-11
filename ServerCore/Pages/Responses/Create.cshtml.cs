using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    public class CreateModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public CreateModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
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