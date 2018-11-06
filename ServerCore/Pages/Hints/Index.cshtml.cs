using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public IList<Hint> Hint { get;set; }

        public int PuzzleID { get; set; }

        public async Task OnGetAsync(int puzzleID)
        {
            PuzzleID = puzzleID;
            Hint = await _context.Hints.Where(hint => hint.Puzzle.ID == puzzleID).ToListAsync();
        }
    }
}
