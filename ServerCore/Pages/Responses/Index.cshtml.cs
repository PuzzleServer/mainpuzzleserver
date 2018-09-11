using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Models;

namespace ServerCore.Pages.Responses
{
    public class IndexModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public IndexModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Response> Responses { get;set; }
        
        public int? PuzzleId { get; set; }

        public int? EventId { get; set; }

        public async Task OnGetAsync(int? puzzleId, int? eventId)
        {
            if (puzzleId != null)
            {
                this.Responses = await _context.Responses.Where((r) => r.Puzzle != null && r.Puzzle.ID == puzzleId).ToListAsync();
                this.PuzzleId = puzzleId;
            }
            else
            {
                this.Responses = await _context.Responses.ToListAsync();
            }

            if (eventId != null)
            {
                this.EventId = eventId;
            }
        }
    }
}
