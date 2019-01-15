using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class SubmitFeedbackModel : EventSpecificPageModel
    {
        public SubmitFeedbackModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public Feedback Feedback { get; set; }  
        public Puzzle Puzzle { get; set; }

        /// <summary>
        /// Gets the submit feedback page for a puzzle
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {       
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound();
            }

            return Page();
        }
        
        /// <summary>
        /// Takes the filled out items and adds it to the database as a new piece of feedback.
        /// </summary>
        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Feedback.Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();

            if (Feedback.Puzzle == null) 
            { 
                return NotFound(); 
            } 

            Feedback.SubmissionTime = DateTime.UtcNow;
            _context.Feedback.Add(Feedback);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Teams/Play");
        }
    }
}