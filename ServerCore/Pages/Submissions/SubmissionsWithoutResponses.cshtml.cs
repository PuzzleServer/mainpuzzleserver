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

            IQueryable<SubmissionCountsView> incorrectCounts = submissionsQ
                .Where(submission => submission.Response == null)
                .GroupBy(submission => Tuple.Create<string,int>
                (
                    submission.SubmissionText,
                    submission.PuzzleID
                ))
                .Select((submissions) => new SubmissionCountsView()
                {
                    PuzzleID = submissions.First().PuzzleID,
                    PuzzleName = submissions.First().Puzzle.Name,
                    SubmissionText = submissions.Key.Item1,
                    NumberOfTimesSubmitted = submissions.Count()
                })
                .OrderByDescending(incorrectCountView => incorrectCountView.NumberOfTimesSubmitted);

            SubmissionCounts = await incorrectCounts.ToListAsync();
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