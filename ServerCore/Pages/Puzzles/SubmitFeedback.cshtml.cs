using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
//    [Authorize(Policy = "PlayerIsOnTeam, PlayerCanSeePuzzle")] // TODO: These auth checks not working currently.
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

            // Seed existing feedback page
            Feedback = await _context.Feedback.Where((f) => f.Puzzle.ID == puzzleId && f.Submitter == LoggedInUser).FirstOrDefaultAsync();
            if (Feedback == null)
            {
                Feedback = new Feedback();
                Feedback.Fun = 5;
                Feedback.Difficulty = 5;
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

            Feedback editableFeedback = await _context.Feedback.Where((f) => f.Puzzle.ID == puzzleId && f.Submitter == LoggedInUser).FirstOrDefaultAsync();
            if (editableFeedback == null)
            {
                Feedback.SubmissionTime = DateTime.UtcNow;
                Feedback.Submitter = LoggedInUser;
                Feedback.Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
                if (Feedback.Puzzle == null)
                {
                    return NotFound();
                }
                _context.Feedback.Add(Feedback);
            }
            else
            {
                editableFeedback.SubmissionTime = DateTime.UtcNow;
                editableFeedback.Submitter = LoggedInUser;
                editableFeedback.Difficulty = Feedback.Difficulty;
                editableFeedback.Fun = Feedback.Fun;
                editableFeedback.WrittenFeedback = Feedback.WrittenFeedback;
                editableFeedback.Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
                if (editableFeedback.Puzzle == null)
                {
                    return NotFound();
                }
                _context.Attach(editableFeedback).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/Teams/Play");
        }
    }
}