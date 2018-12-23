using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.ModelBases
{
    public abstract class PuzzleStatePerTeamPageModel : EventSpecificPageModel
    {
        public SortOrder? Sort { get; set; }

        public PuzzleStatePerTeamPageModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<PuzzleStatePerTeam> PuzzleStatePerTeam { get; set; }

        protected abstract SortOrder DefaultSort { get; }

        public async Task<IActionResult> InitializeModelAsync(Puzzle puzzle, Team team, SortOrder? sort)
        {
            if (puzzle == null && team == null)
            {
                return NotFound();
            }

            if (puzzle != null && !await CanAdminPuzzle(puzzle))
            {
                return NotFound();
            }

            IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper.GetFullReadOnlyQuery(_context, Event, puzzle, team, EventRole == EventRole.admin ? null : LoggedInUser);
            Sort = sort;

            switch(sort ?? DefaultSort)
            {
                case SortOrder.PuzzleAscending:
                    statesQ = statesQ.OrderBy(state => state.Puzzle.Name);
                    break;
                case SortOrder.PuzzleDescending:
                    statesQ = statesQ.OrderByDescending(state => state.Puzzle.Name);
                    break;
                case SortOrder.TeamAscending:
                    statesQ = statesQ.OrderBy(state => state.Team.Name);
                    break;
                case SortOrder.TeamDescending:
                    statesQ = statesQ.OrderByDescending(state => state.Team.Name);
                    break;
                case SortOrder.UnlockAscending:
                    statesQ = statesQ.OrderBy(state => state.UnlockedTime ?? DateTime.MaxValue);
                    break;
                case SortOrder.UnlockDescending:
                    statesQ = statesQ.OrderByDescending(state => state.UnlockedTime ?? DateTime.MaxValue);
                    break;
                case SortOrder.SolveAscending:
                    statesQ = statesQ.OrderBy(state => state.SolvedTime ?? DateTime.MaxValue);
                    break;
                case SortOrder.SolveDescending:
                    statesQ = statesQ.OrderByDescending(state => state.SolvedTime ?? DateTime.MaxValue);
                    break;
                default:
                    throw new ArgumentException($"unknown sort: {sort}");
            }

            PuzzleStatePerTeam = await statesQ.ToListAsync();

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

        public async Task SetUnlockStateAsync(Puzzle puzzle, Team team, bool value)
        {
            if (!await CanAdminPuzzle(puzzle))
            {
                return;
            }

            await PuzzleStateHelper.SetUnlockStateAsync(_context, Event, puzzle, team, value ? (DateTime?)DateTime.UtcNow : null, EventRole == EventRole.admin ? null : LoggedInUser);
        }

        public async Task SetSolveStateAsync(Puzzle puzzle, Team team, bool value)
        {
            if (!await CanAdminPuzzle(puzzle))
            {
                return;
            }

            await PuzzleStateHelper.SetSolveStateAsync(_context, Event, puzzle, team, value ? (DateTime?)DateTime.UtcNow : null, EventRole == EventRole.admin ? null : LoggedInUser);
        }

        public enum SortOrder
        {
            PuzzleAscending,
            PuzzleDescending,
            TeamAscending,
            TeamDescending,
            UnlockAscending,
            UnlockDescending,
            SolveAscending,
            SolveDescending
        }
    }
}
