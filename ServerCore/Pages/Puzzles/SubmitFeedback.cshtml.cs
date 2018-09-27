using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class SubmitFeedbackModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public SubmitFeedbackModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Feedback Feedback { get; set; }  
        public Puzzle Puzzle { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {       
            Puzzle = await _context.Puzzles.Where(m => m.ID == id).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Feedback.Puzzle = await _context.Puzzles.Where(m => m.ID == id).FirstOrDefaultAsync();
            Feedback.SubmissionTime = DateTime.UtcNow;
            _context.Feedback.Add(Feedback);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Feedback", new {id = Feedback.Puzzle.ID});
        }
    }
}