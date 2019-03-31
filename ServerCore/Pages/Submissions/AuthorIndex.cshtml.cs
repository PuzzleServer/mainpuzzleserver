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

        public List<SubmissionView> Submissions { get; set; }

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
                    IQueryable<Submission> submissionsQ;

                    if (teamId == null)
                    {
                        submissionsQ = _context.Submissions.Where((s) => s.Puzzle.Event == Event);
                    }
                    else
                    {
                        submissionsQ = _context.Submissions.Where((s) => s.Team.ID == teamId);
                    }

                    Submissions = await submissionsQ
                        .Select((s) => new SubmissionView { SubmitterName = s.Submitter.Name, PuzzleID = s.Puzzle.ID, PuzzleName = s.Puzzle.Name, TeamID = s.Team.ID, TeamName = s.Team.Name, SubmissionText = s.SubmissionText, ResponseText = s.Response == null ? null : s.Response.ResponseText, TimeSubmitted = s.TimeSubmitted })
                        .ToListAsync();
                }
                else
                {
                    var submissions = new List<SubmissionView>();

                    if (teamId == null)
                    {
                        List<IEnumerable<SubmissionView>> submissionsList = await UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                            .Select((p) => p.Submissions.Select((s) => new SubmissionView { SubmitterName = s.Submitter.Name, PuzzleID = s.Puzzle.ID, PuzzleName = s.Puzzle.Name, TeamID = s.Team.ID, TeamName = s.Team.Name, SubmissionText = s.SubmissionText, ResponseText = s.Response == null ? null : s.Response.ResponseText, TimeSubmitted = s.TimeSubmitted }))
                            .ToListAsync();

                        foreach (var list in submissionsList)
                        {
                            submissions.AddRange(list);
                        }
                    }
                    else
                    {
                        List<IEnumerable<SubmissionView>> submissionsList = await UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                            .Select((p) => p.Submissions.Where((s) => s.Team.ID == teamId).Select((s) => new SubmissionView { SubmitterName = s.Submitter.Name, PuzzleID = s.Puzzle.ID, PuzzleName = s.Puzzle.Name, TeamID = s.Team.ID, TeamName = s.Team.Name, SubmissionText = s.SubmissionText, ResponseText = s.Response == null ? null : s.Response.ResponseText, TimeSubmitted = s.TimeSubmitted }))
                            .ToListAsync();

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

                if (EventRole == EventRole.author && !await UserEventHelper.IsAuthorOfPuzzle(_context, Puzzle, LoggedInUser))
                {
                    return Forbid();
                }

                IQueryable<Submission> submissionsQ;

                if (teamId == null)
                {
                    submissionsQ = _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId);
                }
                else
                {
                    submissionsQ = _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId && s.Team.ID == teamId);
                }

                Submissions = await submissionsQ
                    .Select((s) => new SubmissionView { SubmitterName = s.Submitter.Name, PuzzleID = s.Puzzle.ID, PuzzleName = s.Puzzle.Name, TeamID = s.Team.ID, TeamName = s.Team.Name, SubmissionText = s.SubmissionText, ResponseText = s.Response == null ? null : s.Response.ResponseText, TimeSubmitted = s.TimeSubmitted })
                    .ToListAsync();
            }

            if (teamId != null)
            {
                Team = await _context.Teams.Where(m => m.ID == teamId).FirstOrDefaultAsync();
            }

            switch (sort ?? DefaultSort)
            {
                case SortOrder.PlayerAscending:
                    Submissions.Sort((a, b) => a.SubmitterName.CompareTo(b.SubmitterName));
                    break;
                case SortOrder.PlayerDescending:
                    Submissions.Sort((a, b) => -a.SubmitterName.CompareTo(b.SubmitterName));
                    break;
                case SortOrder.TeamAscending:
                    Submissions.Sort((a, b) => a.TeamName.CompareTo(b.TeamName));
                    break;
                case SortOrder.TeamDescending:
                    Submissions.Sort((a, b) => -a.TeamName.CompareTo(b.TeamName));
                    break;
                case SortOrder.PuzzleAscending:
                    Submissions.Sort((a, b) => a.PuzzleName.CompareTo(b.PuzzleName));
                    break;
                case SortOrder.PuzzleDescending:
                    Submissions.Sort((a, b) => -a.PuzzleName.CompareTo(b.PuzzleName));
                    break;
                case SortOrder.ResponseAscending:
                    Submissions.Sort((a, b) => a.ResponseText.CompareTo(b.ResponseText));
                    break;
                case SortOrder.ResponseDescending:
                    Submissions.Sort((a, b) => -a.ResponseText.CompareTo(b.ResponseText));
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