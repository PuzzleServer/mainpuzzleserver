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
    public class SinglePlayerPuzzleAuthorIndexModel : EventSpecificPageModel
    {
        public class SubmissionView
        {
            public string SubmitterName;
            public int PuzzleID;
            public string PuzzleName;
            public string ResponseText;
            public string SubmissionText;
            public DateTime TimeSubmitted;
            public bool IsFreeform;
            public bool Linkify;
        }

        public SinglePlayerPuzzleAuthorIndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IEnumerable<SubmissionView> Submissions { get; set; }

        public Puzzle Puzzle { get; set; }

        public string PlayerName { get; set; }

        public int? PlayerId { get; set; }

        public SortOrder? Sort { get; set; }

        public const SortOrder DefaultSort = SortOrder.TimeDescending;

        public bool HideFreeform { get; set; }

        public bool FavoritesOnly { get; set; }

        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? playerId, SortOrder? sort, bool? hideFreeform, bool? favoritesOnly)
        {
            Sort = sort;
            PlayerId = playerId;
            if (playerId.HasValue)
            {
                PuzzleUser user = await _context.PuzzleUsers.Where(m => m.ID == playerId).FirstOrDefaultAsync();
                PlayerName = user?.Name ?? user?.Email;
            }

            IQueryable<SinglePlayerPuzzleSubmission> submissionsQ = null;

            if (puzzleId == null)
            {
                if (EventRole == EventRole.admin)
                {
                    if (puzzleId == null)
                    {
                        submissionsQ = _context.SinglePlayerPuzzleSubmissions.Where((s) => s.Puzzle.Event == Event);
                    }
                    else
                    {
                        submissionsQ = _context.SinglePlayerPuzzleSubmissions.Where((s) => s.SubmitterID == playerId);
                    }
                }
                else
                {
                    if (playerId == null)
                    {
                        submissionsQ = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                            .SelectMany((p) => p.SinglePlayerPuzzleSubmissions);
                    }
                    else
                    {
                        submissionsQ = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                            .Where((p) => p.IsForSinglePlayer)
                            .SelectMany((p) => p.SinglePlayerPuzzleSubmissions.Where((s) => s.SubmitterID == playerId));
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

                if (playerId == null)
                {
                    submissionsQ = _context.SinglePlayerPuzzleSubmissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId);
                }
                else
                {
                    submissionsQ = _context.SinglePlayerPuzzleSubmissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId && s.SubmitterID == playerId);
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

            switch (sort ?? DefaultSort)
            {
                case SortOrder.PlayerAscending:
                    submissionsQ = submissionsQ.OrderBy(submission => submission.Submitter.Name);
                    break;
                case SortOrder.PlayerDescending:
                    submissionsQ = submissionsQ.OrderByDescending(submission => submission.Submitter.Name);
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
            }

            IQueryable<SubmissionView> submissionViewQ = submissionsQ
                .Select((s) => new SubmissionView
                {
                    SubmitterName = s.Submitter.Name,
                    PuzzleID = s.Puzzle.ID,
                    PuzzleName = s.Puzzle.PlaintextName,
                    IsFreeform = s.Puzzle.IsFreeform,
                    SubmissionText = s.Puzzle.IsFreeform ? s.UnformattedSubmissionText : s.SubmissionText,
                    ResponseText = s.Response == null ? null : s.Response.ResponseText,
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