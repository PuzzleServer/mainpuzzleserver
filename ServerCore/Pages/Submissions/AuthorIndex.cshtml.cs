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
    public class AuthorIndexModel : EventSpecificPageModel
    {
        public class SubmissionView
        {
            public string SubmitterName;
            public int PuzzleID;
            public string PuzzleName;
            public int TeamID;
            public string TeamName;
            public string ResponseText;
            public string SubmissionText;
            public DateTime TimeSubmitted;
        }

        public AuthorIndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IEnumerable<SubmissionView> Submissions { get; set; }

        public Puzzle Puzzle { get; set; }

        public Team  Team { get; set; }

        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? teamId)
        {
            IQueryable<Submission> submissionsQ = null;

            if (puzzleId == null)
            {
                if (EventRole == EventRole.admin)
                {
                    if (teamId == null)
                    {
                        submissionsQ = _context.Submissions.Where((s) => s.Puzzle.Event == Event);
                    }
                    else
                    {
                        submissionsQ = _context.Submissions.Where((s) => s.Team.ID == teamId);
                    }
                }
                else
                {
                    if (teamId == null)
                    {
                        submissionsQ = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                            .SelectMany((p) => p.Submissions);
                    }
                    else
                    {
                        submissionsQ = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                            .SelectMany((p) => p.Submissions.Where((s) => s.Team.ID == teamId));
                    }
                }
            }
            else
            {
                Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();

                if (EventRole == EventRole.author && !await UserEventHelper.IsAuthorOfPuzzle(_context, Puzzle, LoggedInUser))
                {
                    return Forbid();
                }

                if (teamId == null)
                {
                    submissionsQ = _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId);
                }
                else
                {
                    submissionsQ = _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId && s.Team.ID == teamId);
                }
            }
            
            IQueryable<SubmissionView> submissionViewQ = submissionsQ
                .OrderByDescending(s => s.TimeSubmitted)
                .Select((s) => new SubmissionView
                {
                    SubmitterName = s.Submitter.Name,
                    PuzzleID = s.Puzzle.ID,
                    PuzzleName = s.Puzzle.Name,
                    TeamID = s.Team.ID,
                    TeamName = s.Team.Name,
                    SubmissionText = s.SubmissionText,
                    ResponseText = s.Response == null ? null : s.Response.ResponseText,
                    TimeSubmitted = s.TimeSubmitted
                });

            if (teamId != null)
            {
                Team = await _context.Teams.Where(m => m.ID == teamId).FirstOrDefaultAsync();
            }

            Submissions = await submissionViewQ.ToListAsync();
            return Page();
        }
    }
}