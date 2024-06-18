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
            public bool IsFreeform;
            public bool Linkify;
            public bool IsSolution;
        }

        public AuthorIndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IEnumerable<SubmissionView> Submissions { get; set; }

        public Puzzle Puzzle { get; set; }

        public Team  Team { get; set; }

        public SortOrder? Sort { get; set; }

        public const SortOrder DefaultSort = SortOrder.TimeDescending;

        public bool HideFreeform { get; set; }

        public bool FavoritesOnly { get; set; }

        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? teamId, SortOrder? sort, bool? hideFreeform, bool? favoritesOnly)
        {
            Sort = sort;

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
                if (Puzzle.IsForSinglePlayer)
                {
                    return RedirectToPage("/Submissions/SinglePlayerPuzzleAuthorIndex", new { puzzleId = puzzleId });
                }

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

            if (hideFreeform == null || hideFreeform.Value)
            {
                HideFreeform = true;
                submissionsQ = submissionsQ.Where(s => !s.Puzzle.IsFreeform);
            }
            else
            {
                HideFreeform = false;
            }

            if (favoritesOnly.HasValue && favoritesOnly.Value)
            {
                FavoritesOnly = true;
                submissionsQ = submissionsQ.Where(s => s.FreeformFavorited);
            }

            if (teamId != null)
            {
                Team = await _context.Teams.Where(m => m.ID == teamId).FirstOrDefaultAsync();
            }

            switch (sort ?? DefaultSort)
            {
                case SortOrder.PlayerAscending:
                    submissionsQ = submissionsQ.OrderBy(submission => submission.Submitter.Name);
                    break;
                case SortOrder.PlayerDescending:
                    submissionsQ = submissionsQ.OrderByDescending(submission => submission.Submitter.Name);
                    break;
                case SortOrder.TeamAscending:
                    submissionsQ = submissionsQ.OrderBy(submission => submission.Team.Name);
                    break;
                case SortOrder.TeamDescending:
                    submissionsQ = submissionsQ.OrderByDescending(submission => submission.Team.Name);
                    break;
                case SortOrder.PuzzleAscending:
                    submissionsQ = submissionsQ.OrderBy(submission => submission.Puzzle.Name);
                    break;
                case SortOrder.PuzzleDescending:
                    submissionsQ = submissionsQ.OrderByDescending(submission => submission.Puzzle.Name);
                    break;
                case SortOrder.ResponseAscending:
                    submissionsQ = submissionsQ.OrderBy(submission => submission.Response.ResponseText);
                    break;
                case SortOrder.ResponseDescending:
                    submissionsQ = submissionsQ.OrderByDescending(submission => submission.Response.ResponseText);
                    break;
                case SortOrder.SubmissionAscending:
                    submissionsQ = submissionsQ.OrderBy(submission => submission.SubmissionText);
                    break;
                case SortOrder.SubmissionDescending:
                    submissionsQ = submissionsQ.OrderByDescending(submission => submission.SubmissionText);
                    break;
                case SortOrder.TimeAscending:
                    submissionsQ = submissionsQ.OrderBy(submission => submission.TimeSubmitted);
                    break;
                case SortOrder.TimeDescending:
                    submissionsQ = submissionsQ.OrderByDescending(submission => submission.TimeSubmitted);
                    break;
                case SortOrder.IsSolutionAscending:
                    submissionsQ = submissionsQ.OrderBy(submission => submission.Response.IsSolution);
                    break;
                case SortOrder.IsSolutionDescending:
                    submissionsQ = submissionsQ.OrderByDescending(submission => submission.Response.IsSolution);
                    break;
            }

            IQueryable<SubmissionView> submissionViewQ = submissionsQ
                .Select((s) => new SubmissionView
                {
                    SubmitterName = s.Submitter.Name,
                    PuzzleID = s.Puzzle.ID,
                    PuzzleName = s.Puzzle.Name,
                    TeamID = s.Team.ID,
                    TeamName = s.Team.Name,
                    IsFreeform = s.Puzzle.IsFreeform,
                    SubmissionText = s.Puzzle.IsFreeform ? s.UnformattedSubmissionText : s.SubmissionText,
                    ResponseText = s.Response == null ? null : s.Response.ResponseText,
                    IsSolution = s.Response == null ? false : s.Response.IsSolution,
                    TimeSubmitted = s.TimeSubmitted
                });

            Submissions = await submissionViewQ.ToListAsync();

            if (!HideFreeform)
            {
                foreach (SubmissionView view in Submissions)
                {
                    if (view.IsFreeform)
                    {
                        if (Uri.IsWellFormedUriString(view.SubmissionText, UriKind.Absolute))
                        {
                            view.Linkify = true;
                        }
                    }
                }
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
            TimeDescending,
            IsSolutionAscending,
            IsSolutionDescending
        }
    }
}