using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class FeedbackModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public FeedbackModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Feedback> Feedbacks { get; set; }
        public Puzzle Puzzle { get; set; }

        public async Task OnGetAsync(int id)
        {
            Feedbacks = await _context.Feedback.Where((f) => f.Puzzle.ID == id).ToListAsync();
            Puzzle = await _context.Puzzles.Where((p) => p.ID == id).FirstOrDefaultAsync();
        }
    }
}
