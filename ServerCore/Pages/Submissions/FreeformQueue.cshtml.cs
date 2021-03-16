using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Submissions
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class FreeformQueueModel : EventSpecificPageModel
    {
        public FreeformQueueModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager)
            : base(serverContext, userManager)
        {
        }

        public Puzzle Puzzle { get; set; }

        public class SubmissionView
        {
            public int PuzzleId { get; set; }
            public string PuzzleName { get; set; }
            public string SubmissionText { get; set; }
            public int SubmissionId { get; set; }
        }

        [BindProperty]
        public string FreeformResponse { get; set; }

        [BindProperty]
        public int SubmissionID { get; set; }

        public List<SubmissionView> Submissions { get; set; }

        public async Task OnGetAsync(int? puzzleId)
        {
            await PopulateUI(puzzleId);
        }

        private async Task PopulateUI(int? puzzleId)
        {
            if (puzzleId != null)
            {
                Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            }
            //todo add team names to view
            // email teams with results
            // change accept/reject to radio buttons
            // think about a way to go back to old submissions and redo them (e.g. to fix a forgotten comment)
            // todo: should admins be able to see anything they're not an author of?
            var submissionQuery = from Puzzle puzzle in _context.Puzzles
                                  join PuzzleAuthors author in _context.PuzzleAuthors on puzzle.ID equals author.PuzzleID
                                  join Submission submission in _context.Submissions on puzzle.ID equals submission.PuzzleID
                                  join PuzzleStatePerTeam pspt in _context.PuzzleStatePerTeam on new { PuzzleID = puzzle.ID, TeamID = submission.TeamID } equals new { PuzzleID = pspt.PuzzleID, TeamID = pspt.TeamID }
                                  where puzzle.EventID == Event.ID &&
                                  author.Author == LoggedInUser &&
                                  puzzle.IsFreeform &&
                                  pspt.UnlockedTime != null && pspt.SolvedTime == null &&
                                  submission.FreeformAccepted == null
                                  select new SubmissionView() { PuzzleId = puzzle.ID, PuzzleName = puzzle.Name, SubmissionText = submission.SubmissionText, SubmissionId = submission.ID };
            if (puzzleId != null)
            {
                submissionQuery = submissionQuery.Where(submissionView => submissionView.PuzzleId == puzzleId);
            }

            Submissions = await submissionQuery.ToListAsync();
        }

        public async Task<IActionResult> OnPostAcceptAsync()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                // todo admin access?
                Submission submission = await (from Submission sub in _context.Submissions
                                               join PuzzleAuthors author in _context.PuzzleAuthors on sub.PuzzleID equals author.PuzzleID
                                               where sub.ID == SubmissionID &&
                                               sub.FreeformAccepted == null &&
                                               author.Author == LoggedInUser
                                               select sub).FirstOrDefaultAsync();

                if (submission != null)
                {
                    submission.FreeformAccepted = true;
                    submission.FreeformResponse = FreeformResponse;
                    await PuzzleStateHelper.SetSolveStateAsync(_context, Event, submission.Puzzle, submission.Team, DateTime.UtcNow);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            await PopulateUI(null); //todo: puzzle id
            return Page();
        }

        // todo: refactor to share with accept?
        public async Task<IActionResult> OnPostRejectAsync()
        {
            // todo admin access?
            Submission submission = await (from Submission sub in _context.Submissions
                                           join PuzzleAuthors author in _context.PuzzleAuthors on sub.PuzzleID equals author.PuzzleID
                                           where sub.ID == SubmissionID &&
                                           sub.FreeformAccepted == null &&
                                           author.Author == LoggedInUser
                                           select sub).FirstOrDefaultAsync();

            if (submission != null)
            {
                submission.FreeformAccepted = false;
                submission.FreeformResponse = FreeformResponse;

                await _context.SaveChangesAsync();
            }

            await PopulateUI(null); //todo: puzzle id
            return Page();
        }
    }
}
