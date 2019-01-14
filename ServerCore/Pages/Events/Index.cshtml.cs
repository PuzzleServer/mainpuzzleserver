using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsGlobalAdmin")]
    public class IndexModel : PageModel
    {
        private readonly PuzzleServerContext _context;

        public IndexModel(PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Event> Events { get;set; }

        public async Task OnGetAsync()
        {
            Events = await _context.Events.ToListAsync();
        }
    }
}
