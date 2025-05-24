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
    /// Model for the team's submissions page, shows submissions for all puzzles for one team
    /// </summary>
    [Authorize(Policy = "PlayerIsOnTeam")]
    public class AllSubmissionsModel : EventSpecificPageModel
    {
        // see https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-2.1 to make this sortable!

        public AllSubmissionsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<SubmissionView> AllSubmissions { get; set; }

        public int TeamID { get; set; }

        public SortOrder? Sort { get; set; }

        private const SortOrder DefaultSort = SortOrder.GroupAscending;

        public async Task OnGetAsync(SortOrder? sort, int teamId)
        {
            TeamID = teamId;

            IQueryable<PuzzleStatePerTeam> puzzleStates = _context.PuzzleStatePerTeam
                .Where((state) => state.TeamID == teamId && state.SolvedTime != null && state.Puzzle.IsPuzzle);

            IQueryable<Submission> submissions = _context.Submissions
                .Where((s) => s.TeamID == teamId);

            IQueryable<SubmissionView> finalSubmissions =
                from state in puzzleStates
                join submission in submissions on state.PuzzleID equals submission.PuzzleID into joinedStateSubmission
                from joinedSubmission in joinedStateSubmission.DefaultIfEmpty()
                select new SubmissionView
                {
                    SolvedTime = state.SolvedTime.Value,
                    Group = state.Puzzle.Group,
                    OrderInGroup = state.Puzzle.OrderInGroup,
                    Name = state.Puzzle.Name,
                    SubmissionText = joinedSubmission.SubmissionText,
                    ResponseText = joinedSubmission.Response.ResponseText
                };

            this.Sort = sort;
            switch (sort ?? DefaultSort) {
                case SortOrder.PuzzleNameAscending:
                    finalSubmissions = finalSubmissions.OrderBy(s => s.Name);
                    break;
                case SortOrder.PuzzleNameDescending:
                    finalSubmissions = finalSubmissions.OrderByDescending(s => s.Name);
                    break;
                case SortOrder.SolvedTimeAscending:
                    finalSubmissions = finalSubmissions.OrderBy(s => s.SolvedTime).ThenBy(s => s.Group).ThenBy(s => s.OrderInGroup);
                    break;
                case SortOrder.SolvedTimeDecending:
                    finalSubmissions = finalSubmissions.OrderByDescending(s => s.SolvedTime).ThenByDescending(s => s.Group).ThenByDescending(s => s.OrderInGroup);
                    break;
                case SortOrder.GroupAscending:
                    finalSubmissions = finalSubmissions.OrderBy(s => s.Group).ThenBy(s => s.OrderInGroup);
                    break;
                case SortOrder.GroupDescending:
                    finalSubmissions = finalSubmissions.OrderByDescending(s => s.Group).ThenByDescending(s => s.OrderInGroup);
                    break;
                default:
                    throw new Exception("Sort order is not mapped");
            }

            AllSubmissions = await finalSubmissions.ToListAsync();
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
            SolvedTimeAscending,
            SolvedTimeDecending,
            PuzzleNameAscending,
            PuzzleNameDescending,
            GroupAscending,
            GroupDescending
        }

        public class SubmissionView
        {
            public DateTime SolvedTime { get; set; }
            public string Group { get; set; }
            public int OrderInGroup { get; set; }
            public string Name { get; set; }
            public string SubmissionText { get; set; }
            public string ResponseText { get; set; }
        }
    }
}
