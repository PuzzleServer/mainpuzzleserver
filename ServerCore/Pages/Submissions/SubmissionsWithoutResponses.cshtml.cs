using System;
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

namespace ServerCore.Pages.Submissions
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class SubmissionsWithoutResponsesModel : EventSpecificPageModel
    {
        public SubmissionsWithoutResponsesModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager)
            : base(serverContext, userManager)
        {
        }

        public IEnumerable<SubmissionCountsView> SubmissionCounts { get; set; }

        public Puzzle Puzzle { get; set; }

        public class SubmissionCountsView
        {
            public int PuzzleID { get; set; }
            public string PuzzleName { get; set; }
            public string SubmissionText { get; set; }
            public int NumberOfTimesSubmitted { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? puzzleId)
        {
            IQueryable<Submission> submissionsQ = null;

            if (puzzleId == null)
            {
                if (EventRole == EventRole.admin)
                {
                    submissionsQ = _context.Submissions.Where((s) => s.Puzzle.Event == Event);

                }
                else
                {
                    submissionsQ = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                        .SelectMany((p) => p.Submissions);
                }
            }
            else
            {
                Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();

                if (EventRole == EventRole.author && !await UserEventHelper.IsAuthorOfPuzzle(_context, Puzzle, LoggedInUser))
                {
                    return Forbid();
                }

                submissionsQ = _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId);
            }

            SubmissionCounts = await (from submission in submissionsQ
                                         where submission.Response == null
                                         group submission by new { submission.PuzzleID, submission.SubmissionText, submission.Puzzle.Name } into submissionCounts
                                         orderby submissionCounts.Count() descending
                                         select new SubmissionCountsView
                                         {
                                             PuzzleID = submissionCounts.Key.PuzzleID,
                                             PuzzleName = submissionCounts.Key.Name,
                                             SubmissionText = submissionCounts.Key.SubmissionText,
                                             NumberOfTimesSubmitted = submissionCounts.Count()
                                         }                                         
                                         ).ToListAsync();
            return Page();
        }

        private class TupleComparer : IEqualityComparer<Tuple<string, int>>
        {
            public bool Equals(Tuple<string, int> x, Tuple<string, int> y)
            {
                return x.Item1 == y.Item1 && x.Item2 == y.Item2;
            }

            public int GetHashCode(Tuple<string, int> obj)
            {
                return obj.Item2;
            }
        }
    }
}