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

        public List<Submission> Submissions { get; set; }

        public Puzzle Puzzle { get; set; }

        public Team  Team { get; set; }

        public SortOrder? Sort { get; set; }

        public const SortOrder DefaultSort = SortOrder.TimeDescending;

        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? teamId, SortOrder? sort)
        {
            Sort = sort;

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

            switch (sort ?? DefaultSort)
            {
                case SortOrder.PlayerAscending:
                    Submissions.Sort((a, b) => a.Submitter.Name.CompareTo(b.Submitter.Name));
                    break;
                case SortOrder.PlayerDescending:
                    Submissions.Sort((a, b) => -a.Submitter.Name.CompareTo(b.Submitter.Name));
                    break;
                case SortOrder.TeamAscending:
                    Submissions.Sort((a, b) => a.Team.Name.CompareTo(b.Team.Name));
                    break;
                case SortOrder.TeamDescending:
                    Submissions.Sort((a, b) => -a.Team.Name.CompareTo(b.Team.Name));
                    break;
                case SortOrder.PuzzleAscending:
                    Submissions.Sort((a, b) => a.Puzzle.Name.CompareTo(b.Puzzle.Name));
                    break;
                case SortOrder.PuzzleDescending:
                    Submissions.Sort((a, b) => -a.Puzzle.Name.CompareTo(b.Puzzle.Name));
                    break;
                case SortOrder.ResponseAscending:
                    Submissions.Sort((a, b) => a.Response.ResponseText.CompareTo(b.Response.ResponseText));
                    break;
                case SortOrder.ResponseDescending:
                    Submissions.Sort((a, b) => -a.Response.ResponseText.CompareTo(b.Response.ResponseText));
                    break;
                case SortOrder.SubmissionAscending:
                    Submissions.Sort((a, b) => a.SubmissionText.CompareTo(b.SubmissionText));
                    break;
                case SortOrder.SubmissionDescending:
                    Submissions.Sort((a, b) => -a.SubmissionText.CompareTo(b.SubmissionText));
                    break;
                case SortOrder.TimeAscending:
                    Submissions.Sort((a, b) => a.TimeSubmitted.CompareTo(b.TimeSubmitted));
                    break;
                case SortOrder.TimeDescending:
                    Submissions.Sort((a, b) => -a.TimeSubmitted.CompareTo(b.TimeSubmitted));
                    break;
            }

            return Page();
        }

        public SortOrder? SortForColumnLink(SortOrder ascendingSort, SortOrder descendingSort)
        {
            SortOrder result = ascendingSort;

            if (result == (Sort ?? DefaultSort))
            {
                result = descendingSort;
            }

            if (result == DefaultSort)
            {
                return null;
            }

            return result;
        }

        public enum SortOrder
        {
            PlayerAscending,
            PlayerDescending,
            TeamAscending,
            TeamDescending,
            PuzzleAscending,
            PuzzleDescending,
            ResponseAscending,
            ResponseDescending,
            SubmissionAscending,
            SubmissionDescending,
            TimeAscending,
            TimeDescending
        }
    }
}