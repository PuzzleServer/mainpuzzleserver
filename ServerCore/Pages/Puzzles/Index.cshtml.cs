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

        public IList<Puzzle> Puzzle { get;set; }

        public async Task OnGetAsync()
        {
            Puzzle = await _context.Puzzle.ToListAsync();
        }
    }
}
