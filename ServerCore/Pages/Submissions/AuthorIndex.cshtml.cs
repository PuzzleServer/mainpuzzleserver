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
        public AuthorIndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Submission> Submissions { get; set; }

        public Puzzle Puzzle { get; set; }

        public Team  Team { get; set; }

        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? teamId)
        {
            if (puzzleId == null)
            {
                if (EventRole == EventRole.admin)
                {
                    if (teamId == null)
                    {
                        Submissions = await _context.Submissions.ToListAsync();
                    }
                    else
                    {
                        Submissions = await _context.Submissions.Where((s) => s.Team.ID == teamId).ToListAsync();
                    }
                }
                else
                {
                    var submissions = new List<Submission>();

                    if (teamId == null)
                    {
                        List<List<Submission>> submissionsList = await UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser).Select((p) => p.Submissions).ToListAsync();

                        foreach (var list in submissionsList)
                        {
                            submissions.AddRange(list);
                        }
                    }
                    else
                    {
                        List<IEnumerable<Submission>> submissionsList = await UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser).Select((p) => p.Submissions.Where((s) => s.Team.ID == teamId)).ToListAsync();

                        foreach (var list in submissionsList)
                        {
                            submissions.AddRange(list);
                        }
                    }

                    Submissions = submissions;
                }
            }
            else
            {
                Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();

                if (!await UserEventHelper.IsAuthorOfPuzzle(_context, Puzzle, LoggedInUser))
                {
                    return Forbid();
                }

                if (teamId == null)
                {
                    Submissions = await _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId).ToListAsync();
                }
                else
                {
                    Submissions = await _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId && s.Team.ID == teamId).ToListAsync();
                }
            }

            if (teamId != null)
            {
                Team = await _context.Teams.Where(m => m.ID == teamId).FirstOrDefaultAsync();
            }

            return Page();
        }
    }
}