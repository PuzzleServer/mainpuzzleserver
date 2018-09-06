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

        public async Task<IActionResult> OnGetAsync(int eventid)
        {
            Event = await _context.Events.SingleOrDefaultAsync(m => m.ID == eventid);
            ViewData["Event"] = Event;
            return Page();
        }

        [BindProperty]
        public Puzzle Puzzle { get; set; }

        public Event Event { get; set; }

        public async Task<IActionResult> OnPostAsync(int eventid)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Event = await _context.Events.SingleOrDefaultAsync(m => m.ID == eventid);
            Puzzle.Event = Event;
            ViewData["Event"] = Event;

            _context.Puzzles.Add(Puzzle);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { eventid = eventid });
        }
    }
}