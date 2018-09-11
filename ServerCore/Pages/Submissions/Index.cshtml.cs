using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Submissions
{
    public class IndexModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public IndexModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Submission> Submissions { get;set; }

        public int? PuzzleId { get; set; }

        public int? EventId { get; set; }

        public async Task OnGetAsync(int? puzzleId, int? eventId)
        {
            Submissions = await _context.Submissions.ToListAsync();
            
            if (puzzleId != null)
            {
                this.Submissions = await _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId).ToListAsync();
                this.PuzzleId = puzzleId;
            }
            else
            {
                this.Submissions = await _context.Submissions.ToListAsync();
            }

            if (eventId != null)
            {
                this.EventId = eventId;
            }
        }
    }
}
