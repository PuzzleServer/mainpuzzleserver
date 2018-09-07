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
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public IndexModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Puzzle> Puzzles { get; set; }

        public async Task OnGetAsync()
        {
            if (Event != null)
            {
                Puzzles = await _context.Puzzles.Where((p) => p.Event != null && p.Event.ID == Event.ID).ToListAsync();
            }
            else
            {
                Puzzles = await _context.Puzzles.ToListAsync();
            }
        }
    }
}
