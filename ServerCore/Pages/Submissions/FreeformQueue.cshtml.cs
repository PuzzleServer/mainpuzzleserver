using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            public string TeamName { get; set; }
            public string SubmissionText { get; set; }
            public int SubmissionId { get; set; }
            public bool Linkify { get; set; }
            public bool Sharable { get; set; }
        }

        [BindProperty]
        public string FreeformResponse { get; set; }

        [BindProperty]
        public int SubmissionID { get; set; }

        public enum FreeformResult
        {
            None,
            Accepted,
            Rejected
        }

        [BindProperty]
        public FreeformResult Result { get; set; }

        [BindProperty]
        public bool Favorite { get; set; }

        public List<SubmissionView> Submissions { get; set; }

        public int FullQueueSize { get; set; }

        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? refresh)
        {
            if (refresh != null)
            {
                Refresh = refresh;
            }

            if (puzzleId != null)
            {
                Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId && m.EventID == Event.ID).FirstOrDefaultAsync();
                if (Puzzle == null)
                {
                    return NotFound();
                }

                if (EventRole == EventRole.author && !await UserEventHelper.IsAuthorOfPuzzle(_context, Puzzle, LoggedInUser))
                {
                    return Forbid();
                }
            }

            await PopulateUI(puzzleId);
            return Page();
        }

        private async Task PopulateUI(int? puzzleId)
        {
            // todo: think about a way to go back to old submissions and redo them (e.g. to fix a forgotten comment)
            bool isAdmin = (EventRole == EventRole.admin);
            var submissionQuery = from Puzzle puzzle in _context.Puzzles
                                  join Submission submission in _context.Submissions on puzzle.ID equals submission.PuzzleID
                                  join PuzzleStatePerTeam pspt in _context.PuzzleStatePerTeam on new { PuzzleID = puzzle.ID, TeamID = submission.TeamID } equals new { PuzzleID = pspt.PuzzleID, TeamID = pspt.TeamID }
                                  join PuzzleAuthors author in _context.PuzzleAuthors on puzzle.ID equals author.PuzzleID
                                  where puzzle.EventID == Event.ID &&
                                  (isAdmin || author.Author == LoggedInUser) &&
                                  puzzle.IsFreeform &&
                                  pspt.UnlockedTime != null && pspt.SolvedTime == null &&
                                  submission.FreeformAccepted == null
                                  orderby submission.TimeSubmitted
                                  select new SubmissionView()
                                  {
                                      PuzzleId = puzzle.ID,
                                      PuzzleName = puzzle.Name,
                                      TeamName = pspt.Team.Name,
                                      SubmissionText = submission.UnformattedSubmissionText,
                                      SubmissionId = submission.ID,
                                      Sharable = submission.AllowFreeformSharing
                                  };
            if (puzzleId != null)
            {
                submissionQuery = submissionQuery.Where(submissionView => submissionView.PuzzleId == puzzleId);
            }

            Submissions = await submissionQuery.Take(50).ToListAsync();

            FullQueueSize = await submissionQuery.CountAsync();
            
            foreach(SubmissionView submission in Submissions)
            {
                if(submission.SubmissionText.StartsWith("http"))
                {
                    submission.Linkify = Uri.IsWellFormedUriString(submission.SubmissionText, UriKind.Absolute);
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(int? puzzleId)
        {
            // Handle no radio button selected
            if (Result == FreeformResult.None)
            {
                return RedirectToPage();
            }

            bool accepted = (Result == FreeformResult.Accepted);

            if (String.IsNullOrWhiteSpace(FreeformResponse))
            {
                FreeformResponse = Result.ToString();
            }

            Submission submission;
            using (var transaction = _context.Database.BeginTransaction())
            {
                bool isAdmin = (EventRole == EventRole.admin);
                submission = await (from Submission sub in _context.Submissions
                                    join PuzzleAuthors author in _context.PuzzleAuthors on sub.PuzzleID equals author.PuzzleID
                                    where sub.ID == SubmissionID &&
                                    sub.FreeformAccepted == null &&
                                    sub.Puzzle.EventID == Event.ID &&
                                    (isAdmin || author.Author == LoggedInUser)
                                    select sub).FirstOrDefaultAsync();

                if (submission != null)
                {
                    submission.FreeformAccepted = accepted;
                    submission.FreeformResponse = FreeformResponse;
                    submission.FreeformJudge = LoggedInUser;
                    submission.FreeformFavorited = Favorite;

                    if (accepted)
                    {
                        await PuzzleStateHelper.SetSolveStateAsync(_context, Event, submission.Puzzle, submission.Team, DateTime.UtcNow);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }

            if (submission != null)
            {
                MailHelper.Singleton.SendPlaintextOneAddress(submission.Submitter.Email, $"{submission.Puzzle.Name} Submission {Result}", $"Your submission for {submission.Puzzle.Name} has been {Result} with the response: {FreeformResponse}");
            }

            return RedirectToPage();
        }
    }
}
