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

            IQueryable<PuzzleStatePerTeam> puzzleStates = _context.PuzzleStatePerTeam
                .Where((state) => state.TeamID == teamId && state.SolvedTime != null);

            IQueryable<Submission> correctSubmissions = _context.Submissions
                .Where((s) => s.TeamID == teamId && s.Response.IsSolution);

            IQueryable<SubmissionView> finalSubmissions = 
                from state in puzzleStates
                join submission in correctSubmissions on state.PuzzleID equals submission.PuzzleID into joinedStateSubmission
                from joinedSubmission in joinedStateSubmission.DefaultIfEmpty()
                select new SubmissionView
                {
                    SolvedTime = state.SolvedTime.Value,
                    Group = state.Puzzle.Group,
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
                    finalSubmissions = finalSubmissions.OrderBy(s => s.SolvedTime);
                    break;
                case SortOrder.SolvedTimeDecending:
                    finalSubmissions = finalSubmissions.OrderByDescending(s => s.SolvedTime);
                    break;
                case SortOrder.GroupAscending:
                    finalSubmissions = finalSubmissions.OrderBy(s => s.Group);
                    break;
                case SortOrder.GroupDescending:
                    finalSubmissions = finalSubmissions.OrderByDescending(s => s.Group);
                    break;
                default:
                    throw new Exception("Sort order is not mapped");
            }

            CorrectSubmissions = await finalSubmissions.ToListAsync();
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
            public string Name { get; set; }
            public string SubmissionText { get; set; }
            public string ResponseText { get; set; }
        }
    }
}
