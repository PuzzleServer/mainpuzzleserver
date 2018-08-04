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
    public class DetailsModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public DetailsModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public Puzzle Puzzle { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Puzzle = await _context.Puzzle.SingleOrDefaultAsync(m => m.ID == id);

            if (Puzzle == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
