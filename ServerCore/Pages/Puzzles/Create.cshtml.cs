using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Models;

namespace ServerCore.Pages.Puzzles
{
    public class CreateModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public CreateModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Puzzle Puzzle { get; set; }

        public async Task<IActionResult> OnPostAsync(int eventid)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Puzzle.Event = await _context.Event.SingleOrDefaultAsync(m => m.ID == eventid);

            _context.Puzzle.Add(Puzzle);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { eventid = eventid });
        }
    }
}