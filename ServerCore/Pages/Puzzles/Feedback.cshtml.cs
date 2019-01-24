using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;
using System.ComponentModel.DataAnnotations;
namespace ServerCore.Pages.Puzzles
{
    /// <summary>
    /// Model for the author/admin's view of the feedback items for a specific puzzle
    /// /used for viewing and aggregating feedback for a specific puzzle
    /// </summary>
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class FeedbackModel : EventSpecificPageModel
    {
        public FeedbackModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Feedback> Feedbacks { get; set; }
        public Puzzle Puzzle { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public double FunScore;
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public double DiffScore;

        /// <summary>
        /// Gets the feedback and puzzle associated with the given ID
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            Feedbacks = await _context.Feedback.Where((f) => f.Puzzle.ID == puzzleId).Include("Submitter").ToListAsync();
            Puzzle = await _context.Puzzles.Where((p) => p.ID == puzzleId).FirstOrDefaultAsync();

            if (Puzzle == null) 
            { 
                return NotFound("Could not find puzzle for this feedback page."); 
            }
            
            FunScore = 0.0;
            DiffScore = 0.0;
            var count = 0;
            foreach (var item in Feedbacks) {
                FunScore += item.Fun;
                DiffScore += item.Difficulty;
                count++;
            }
            FunScore /= count;
            DiffScore /= count;

            return Page();
        }
    }
}
