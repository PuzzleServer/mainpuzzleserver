using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;
using System.ComponentModel.DataAnnotations;


namespace ServerCore.Pages.Puzzles
{
    /// <summary>
    /// Model for the author/admin's view of the feedback items for a specific puzzle
    /// /used for viewing and aggregating feedback for a specific puzzle
    /// </summary>
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class FeedbackModel : EventSpecificPageModel
    {
        public FeedbackModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public class FeedbackView
        {
            public Puzzle Puzzle { get; set; }
            public Feedback Feedback { get; set; }
            public PuzzleUser Submitter { get; set; }
        }

        public IList<FeedbackView> Feedbacks { get; set; }
        public string PuzzleName { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public double FunScore;
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public double DiffScore;

        /// <summary>
        /// Gets the feedback and puzzle associated with the given ID
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int? puzzleId)
        {
            IQueryable<FeedbackView> feedbackRows;
            if (puzzleId == null)
            {
                IQueryable<Puzzle> puzzles;
                if (EventRole == EventRole.admin)
                {
                    puzzles = from puzzle in _context.Puzzles
                              where puzzle.Event == Event
                              select puzzle;
                }
                else
                {
                    puzzles = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser);
                }
                feedbackRows = from feedback in _context.Feedback
                               join Puzzle puzzle in puzzles on feedback.Puzzle equals puzzle
                               join PuzzleUser submitter in _context.PuzzleUsers on feedback.Submitter equals submitter
                               select new FeedbackView() { Puzzle = puzzle, Feedback = feedback, Submitter = submitter };
            }
            else
            {
                Puzzle puzzle = await (from Puzzle p in _context.Puzzles
                                       where p.ID == puzzleId.Value && p.Event == Event
                                       select p).FirstOrDefaultAsync();
                if (puzzle == null)
                {
                    return NotFound();
                }

                if (EventRole == EventRole.author && !await UserEventHelper.IsAuthorOfPuzzle(_context, puzzle, LoggedInUser))
                {
                    return Forbid();
                }

                PuzzleName = puzzle.Name;

                feedbackRows = from feedback in _context.Feedback
                               join PuzzleUser submitter in _context.PuzzleUsers on feedback.Submitter equals submitter
                               where feedback.Puzzle == puzzle
                               select new FeedbackView() { Puzzle = puzzle, Feedback = feedback, Submitter = submitter };
            }

            Feedbacks = await feedbackRows.ToListAsync();
            
            FunScore = 0.0;
            DiffScore = 0.0;
            var count = 0;
            foreach (var item in Feedbacks) {
                FunScore += item.Feedback.Fun;
                DiffScore += item.Feedback.Difficulty;
                count++;
            }
            FunScore /= count;
            DiffScore /= count;

            return Page();
        }
    }
}
