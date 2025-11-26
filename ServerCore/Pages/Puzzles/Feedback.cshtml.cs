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
        public const int FeedbackMax = Feedback.MaxRating;

        public FeedbackModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public class FeedbackView
        {
            public Puzzle Puzzle { get; set; }
            public Feedback Feedback { get; set; }
            public PuzzleUser Submitter { get; set; }
            public string TeamName { get; set; }
        }

        public IList<FeedbackView> Feedbacks { get; set; }
        public string PuzzleName { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public double FunScore { get; set; }
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public double DiffScore { get; set; }

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

                Dictionary<int, string> playerIdToTeamName = _context.TeamMembers
                    .Where(teamMember => teamMember.Team.Event == Event)
                    .ToDictionary(teamMember => teamMember.Member.ID, teamMember => teamMember.Team.Name);

                feedbackRows = from feedback in _context.Feedback
                               join puzzle in puzzles on feedback.Puzzle equals puzzle
                               where puzzle.Event == Event
                               join submitter in _context.PuzzleUsers on feedback.Submitter equals submitter
                               orderby feedback.SubmissionTime descending
                               select new FeedbackView()
                               {
                                   Puzzle = puzzle,
                                   Feedback = feedback,
                                   Submitter = submitter,
                                   TeamName = playerIdToTeamName.ContainsKey(submitter.ID)
                                       ? playerIdToTeamName[submitter.ID]
                                       : null,
                               };
            }
            else
            {
                Puzzle puzzle = await (from p in _context.Puzzles
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

                PuzzleName = puzzle.PlaintextName;

                Dictionary<int, string> playerIdToTeamName = _context.TeamMembers
                    .Where(teamMember => teamMember.Team.Event == Event)
                    .ToDictionary(teamMember => teamMember.Member.ID, teamMember => teamMember.Team.Name);

                feedbackRows = from feedback in _context.Feedback
                               join submitter in _context.PuzzleUsers on feedback.Submitter equals submitter
                               where feedback.Puzzle == puzzle
                               orderby feedback.SubmissionTime descending
                               select new FeedbackView()
                               {
                                   Puzzle = puzzle,
                                   Feedback = feedback,
                                   Submitter = submitter,
                                   TeamName = playerIdToTeamName.ContainsKey(submitter.ID)
                                       ? playerIdToTeamName[submitter.ID]
                                       : null,
                               };
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
