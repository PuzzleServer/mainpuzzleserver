using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    public class DetailsModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public DetailsModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
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
