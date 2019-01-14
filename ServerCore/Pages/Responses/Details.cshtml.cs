using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class DetailsModel : EventSpecificPageModel
    {
        public DetailsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public Response PuzzleResponse { get; set; }

        public int PuzzleId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PuzzleResponse = await _context.Responses.FirstOrDefaultAsync(m => m.ID == id);

            if (PuzzleResponse == null)
            {
                return NotFound();
            }

            PuzzleId = PuzzleResponse.PuzzleID;
            return Page();
        }
    }
}
