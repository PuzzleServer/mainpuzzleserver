using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsGlobalAdmin")]
    public class DetailsModel : PageModel
    {
        private readonly PuzzleServerContext _context;

        public DetailsModel(PuzzleServerContext context)
        {
            _context = context;
        }

        public Event Event { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Event = await _context.Events.SingleOrDefaultAsync(m => m.ID == id);

            if (Event == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
