using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class IndexModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public IndexModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Puzzle> Puzzles { get; set; }

        public async Task OnGetAsync()
        {
            if (Event != null)
            {
                Puzzles = await _context.Puzzles.Where((p) => p.Event != null && p.Event == Event).ToListAsync();
            }
            else
            {
                Puzzles = await _context.Puzzles.ToListAsync();
            }
        }
    }
}
