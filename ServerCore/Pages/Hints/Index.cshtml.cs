using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
{
    public class IndexModel : EventSpecificPageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public IndexModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Hint> Hints { get;set; }

        public int PuzzleId { get; set; }

        public string PuzzleName { get; set; }

        public async Task OnGetAsync(int puzzleId)
        {
            PuzzleId = puzzleId;
            Hints = await _context.Hints.Where(hint => hint.Puzzle.ID == puzzleId).OrderBy(hint => hint.DisplayOrder).ThenBy(hint => hint.Description).ToListAsync();
            PuzzleName = await _context.Puzzles.Where(puzzle => puzzle.ID == puzzleId).Select(puzzle => puzzle.Name).FirstOrDefaultAsync();
        }
    }
}
