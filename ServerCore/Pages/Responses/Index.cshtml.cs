using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    public class IndexModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public IndexModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Response> Responses { get;set; }
        
        public int PuzzleId { get; set; }

        public async Task OnGetAsync(int puzzleId)
        {
            this.Responses = await _context.Responses.Where((r) => r.Puzzle != null && r.Puzzle.ID == puzzleId).ToListAsync();
            this.PuzzleId = puzzleId;
        }
    }
}
