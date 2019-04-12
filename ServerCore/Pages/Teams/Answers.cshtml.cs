using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    /// <summary>
    /// Model for the player's "Puzzles" page. Shows a list of the team's unsolved puzzles, with sorting options.
    /// </summary>
    [Authorize(Policy = "PlayerIsOnTeam")]
    public class AnswersModel : EventSpecificPageModel
    {
        // see https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-2.1 to make this sortable!

        public AnswersModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<SubmissionView> CorrectSubmissions { get; set; }

        public int TeamID { get; set; }

        public SortOrder? Sort { get; set; }

        private const SortOrder DefaultSort = SortOrder.GroupAscending;

        public async Task OnGetAsync(SortOrder? sort, int teamId)
        {
            TeamID = teamId;

            IQueryable<Submission> submissions = _context.Submissions
                .Where((s) => s.TeamID == teamId && s.Response.IsSolution);

            this.Sort = sort;
            switch (sort ?? DefaultSort) {
                case SortOrder.PuzzleNameAscending:
                    submissions = submissions.OrderBy(s => s.Puzzle.Name);
                    break;
                case SortOrder.PuzzleNameDescending:
                    submissions = submissions.OrderByDescending(s => s.Puzzle.Name);
                    break;
                case SortOrder.TimeSubmittedAscending:
                    submissions = submissions.OrderBy(s => s.TimeSubmitted);
                    break;
                case SortOrder.TimeSubmittedDecending:
                    submissions = submissions.OrderByDescending(s => s.TimeSubmitted);
                    break;
                case SortOrder.GroupAscending:
                    submissions = submissions.OrderBy(s => s.Puzzle.Group);
                    break;
                case SortOrder.GroupDescending:
                    submissions = submissions.OrderByDescending(s => s.Puzzle.Group);
                    break;
                default:
                    throw new Exception("Sort order is not mapped");
            }

            CorrectSubmissions = await submissions
                .Select(s => new SubmissionView()
                {
                    TimeSubmitted = s.TimeSubmitted,
                    Group = s.Puzzle.Group,
                    Name = s.Puzzle.Name,
                    SubmissionText = s.SubmissionText,
                    ResponseText = s.Response.ResponseText
                })
                .ToListAsync();
        }

        public SortOrder? SortForColumnLink(SortOrder ascending, SortOrder descending)
        {
            // Toggle away from the current sort order (descend by default)
            if (descending == (this.Sort ?? DefaultSort))
            {
                return ascending;
            }
            else
            {
                return descending;
            }
        }

        public enum SortOrder
        {
            TimeSubmittedAscending,
            TimeSubmittedDecending,
            PuzzleNameAscending,
            PuzzleNameDescending,
            GroupAscending,
            GroupDescending
        }

        public class SubmissionView
        {
            public DateTime TimeSubmitted { get; set; }
            public string Group { get; set; }
            public string Name { get; set; }
            public string SubmissionText { get; set; }
            public string ResponseText { get; set; }
        }
    }
}
