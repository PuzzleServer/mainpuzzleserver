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
    /// <summary>
    /// Model for the author/admin's view of the feedback items for a specific puzzle
    /// /used for viewing and aggregating feedback for a specific puzzle
    /// </summary>
    public class FeedbackModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public FeedbackModel(PuzzleServerContext context, UserManager<IdentityUser> userManager) : base(context, userManager)
        {
            _context = context;
        }

        public IList<Feedback> Feedbacks { get; set; }
        public Puzzle Puzzle { get; set; }
        public bool IsCurrentUserPuzzleAuthor { get; set; }

        /// <summary>
        /// Gets the feedback and puzzle associated with the given ID
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Feedbacks = await _context.Feedback.Where((f) => f.Puzzle.ID == id).Include("Submitter").ToListAsync();
            Puzzle = await _context.Puzzles.Where((p) => p.ID == id).FirstOrDefaultAsync();

            if (Puzzle == null) 
            { 
                return NotFound(); 
            } 

            IsCurrentUserPuzzleAuthor = (await _context.PuzzleAuthors.Where((a) => a.Puzzle == Puzzle && a.Author == LoggedInUser).FirstOrDefaultAsync() != null);

            return Page();
        }
    }
}
