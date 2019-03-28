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


        public IList<Submission> CorrectSubmissions { get; set; }

        public int TeamID { get; set; }

        public SortOrder? Sort { get; set; }

        private const SortOrder DefaultSort = SortOrder.TimeSubmittedDecending;

        public async Task OnGetAsync(SortOrder? sort, int teamId)
        {
            TeamID = teamId;

            // Verify if the user is on the team in the parametsr
            Team myTeam = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            if (myTeam != null)
            {
                this.TeamID = myTeam.ID;
                await PuzzleStateHelper.CheckForTimedUnlocksAsync(_context, Event, myTeam);
            }
            else
            {
                throw new Exception("Not currently registered for a team");
            }

            IQueryable<Submission> submissions = _context.Submissions
                .Where((s) => s.Team.ID == teamId && s.Response.IsSolution);

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
                default:
                    throw new Exception("Sort order is not mapped");
            }

            CorrectSubmissions = await submissions.ToListAsync();
        }

        public SortOrder? SortForColumnLink(SortOrder ascending, SortOrder descending)
        {
            // Toggle away from the current sort order (ascend by default)
            if (ascending == (this.Sort ?? DefaultSort))
            {
                return descending;
            }
            else
            {
                return ascending;
            }
        }

        public enum SortOrder
        {
            TimeSubmittedAscending,
            TimeSubmittedDecending,
            PuzzleNameAscending,
            PuzzleNameDescending
        }
    }
}
