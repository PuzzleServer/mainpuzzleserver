using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Models;

namespace ServerCore.Pages.Puzzles
{
    public class IndexModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public IndexModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Puzzle> Puzzles { get; set; }

        public int? EventId { get; set; }

        public async Task OnGetAsync(int? eventid)
        {
            if (eventid != null)
            {
                Puzzles = await _context.Puzzles.Where((p) => p.Event != null && p.Event.ID == eventid).ToListAsync();
                EventId = eventid;
                ViewData["Event"] = await _context.Events.Where(e => e.ID == eventid).FirstAsync();
            }
            else
            {
                Puzzles = await _context.Puzzles.ToListAsync();
            }
        }
    }
}
