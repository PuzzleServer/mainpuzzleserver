using System;
using System.Collections.Generic;
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
        private readonly PuzzleServerContext _context;

        public SubmitFeedbackModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
            _context = serverContext;
        }

        [BindProperty]
        public Feedback Feedback { get; set; }  
        public Puzzle Puzzle { get; set; }

        /// <summary>
        /// Gets the submit feedback page for a puzzle
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {       
            Puzzle = await _context.Puzzles.Where(m => m.ID == id).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound();
            }

            return Page();
        }
        
        /// <summary>
        /// Takes the filled out items and adds it to the database as a new piece of feedback.
        /// </summary>
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Feedback.Puzzle = await _context.Puzzles.Where(m => m.ID == id).FirstOrDefaultAsync();

            if (Feedback.Puzzle == null)
            { 
                return NotFound(); 
            } 

            Feedback fromThisUser = await _context.Feedback.Where((f) => f.Puzzle.ID == id && f.Submitter == LoggedInUser).FirstOrDefaultAsync();
            // If there is already feedback from this user update it instead of overwriting.
            if (fromThisUser != null)
            {
                fromThisUser.SubmissionTime = DateTime.UtcNow;
                fromThisUser.WrittenFeedback = fromThisUser.WrittenFeedback + "\n" + Feedback.WrittenFeedback;
                fromThisUser.Difficulty = Feedback.Difficulty;
                fromThisUser.Fun = Feedback.Fun;
                _context.Feedback.Update(fromThisUser);
            }
            // Otherwise create new feedback from this user.
            else
            {
                Feedback.SubmissionTime = DateTime.UtcNow;
                Feedback.Submitter = LoggedInUser;
                _context.Feedback.Add(Feedback);
            }
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}