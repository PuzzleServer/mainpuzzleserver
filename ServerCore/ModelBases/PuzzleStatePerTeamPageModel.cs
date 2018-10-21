using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.ModelBases
{
    public abstract class PuzzleStatePerTeamPageModel : EventSpecificPageModel
    {
        protected PuzzleServerContext Context { get; }

        public SortOrder? Sort { get; set; }

        public PuzzleStatePerTeamPageModel(PuzzleServerContext context)
        {
            Context = context;
        }

        public IList<PuzzleStatePerTeam> PuzzleStatePerTeam { get; set; }

        protected abstract SortOrder DefaultSort { get; }

        public async Task InitializeModelAsync(Puzzle puzzle, Team team, SortOrder? sort)
        {
            IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper.GetFullReadOnlyQuery(Context, Event, puzzle, team);
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

        public Task SetUnlockStateAsync(Puzzle puzzle, Team team, bool value)
        {
            return PuzzleStateHelper.SetUnlockStateAsync(Context, Event, puzzle, team, value ? (DateTime?)DateTime.UtcNow : null);
        }

        public Task SetSolveStateAsync(Puzzle puzzle, Team team, bool value)
        {
            return PuzzleStateHelper.SetSolveStateAsync(Context, Event, puzzle, team, value ? (DateTime?)DateTime.UtcNow : null);
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
