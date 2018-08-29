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
    public class DeleteModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public DeleteModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Puzzle Puzzle { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Puzzle = await _context.Puzzle.Where(m => m.ID == id).Include(p => p.Event).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Puzzle = await _context.Puzzle.Where(m => m.ID == id).Include(p => p.Event).FirstOrDefaultAsync();

            if (Puzzle != null)
            {
                _context.Puzzle.Remove(Puzzle);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index", new { eventid = Puzzle.Event?.ID });
        }
    }
}
