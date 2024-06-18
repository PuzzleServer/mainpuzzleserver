using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class SubmissionView
    {
        public string TeamName;
        public string SubmissionText;
        public bool Linkify;
    }

    [Authorize(Policy = "IsRegisteredForEvent")]
    public class PlayerSubmissionsModel : EventSpecificPageModel
    {
        /// <summary>
        /// True if content isn't available for the user yet
        /// </summary>
        public bool NoContent { get; set; }

        /// <summary>
        /// List of puzzles to choose
        /// </summary>
        public List<SelectListItem> FreeformPuzzles { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// /Submissions for the current puzzle
        /// </summary>
        public List<SubmissionView> Submissions { get; set; } = new List<SubmissionView>();

        /// <summary>
        /// Current puzzle
        /// </summary>
        public String PuzzleName { get; set; }

        public PlayerSubmissionsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(int? puzzleId)
        {
            // Hide submissions from players until the event is over
            if (EventRole != EventRole.admin && EventRole != EventRole.author && DateTime.UtcNow <= Event.AnswersAvailableBegin)
            {
                NoContent = true;
                return Page();
            }

            var puzzleQuery = from puzzle in _context.Puzzles
                              where puzzle.EventID == Event.ID &&
                              puzzle.IsFreeform
                              select puzzle;

            if (EventRole == EventRole.author)
            {
                puzzleQuery = from puzzle in puzzleQuery
                              join puzzleAuthor in _context.PuzzleAuthors on puzzle.ID equals puzzleAuthor.PuzzleID
                              where puzzleAuthor.AuthorID == LoggedInUser.ID
                              select puzzle;
            }

            FreeformPuzzles = await (puzzleQuery.Select(puzzle => new SelectListItem()
                                     {
                                         Text = puzzle.Name,
                                         Value = puzzle.ID.ToString(),
                                         Selected = (puzzleId != null && puzzle.ID == puzzleId.Value)
                                     })).ToListAsync();

            if (puzzleId == null)
            {
                return Page();
            }

            Puzzle selectedPuzzle = await (from puzzle in puzzleQuery
                                           where puzzle.ID == puzzleId
                                           select puzzle).FirstOrDefaultAsync();

            if (selectedPuzzle == null)
            {
                return NotFound();
            }

            PuzzleName = selectedPuzzle.Name;

            Submissions = await (from submission in _context.Submissions
                                 join puzzle in _context.Puzzles on submission.PuzzleID equals puzzle.ID
                                 join team in _context.Teams on submission.TeamID equals team.ID
                                 where puzzle.ID == puzzleId &&
                                 puzzle.EventID == Event.ID &&
                                 puzzle.IsFreeform &&
                                 submission.AllowFreeformSharing &&
                                 submission.FreeformAccepted.HasValue &&
                                 submission.FreeformAccepted.Value
                                 orderby team.Name
                                 select new SubmissionView()
                                 {
                                     SubmissionText = submission.UnformattedSubmissionText,
                                     TeamName = team.Name,
                                     Linkify = Uri.IsWellFormedUriString(submission.UnformattedSubmissionText, UriKind.Absolute)
                                 }
                                 ).ToListAsync();

            return Page();
        }
    }
}
