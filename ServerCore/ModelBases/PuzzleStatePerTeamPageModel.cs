using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Models;

namespace ServerCore.ModelBases
{
    public abstract class PuzzleStatePerTeamPageModel : EventSpecificPageModel
    {
        protected PuzzleServerContext Context { get; }

        public SortOrder? Sort { get; set; }

        public PuzzleStatePerTeamPageModel(ServerCore.Models.PuzzleServerContext context)
        {
            this.Context = context;
        }

        public IList<PuzzleStatePerTeam> PuzzleStatePerTeam { get; set; }

        protected abstract SortOrder DefaultSort { get; }

        public async Task InitializeModelAsync(Puzzle puzzle, Team team, SortOrder? sort)
        {
            IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper.GetFullReadOnlyQuery(this.Context, this.Event, puzzle, team);
            this.Sort = sort;

            switch(sort ?? this.DefaultSort)
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

            if (result == (this.Sort ?? DefaultSort))
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
            var statesQ = await PuzzleStateHelper.GetFullReadWriteQueryAsync(this.Context, this.Event, puzzle, team);
            var states = await statesQ.ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                states[i].IsUnlocked = value;
            }
            await Context.SaveChangesAsync();
        }

        public async Task SetSolveStateAsync(Puzzle puzzle, Team team, bool value)
        {
            var statesQ = await PuzzleStateHelper.GetFullReadWriteQueryAsync(this.Context, this.Event, puzzle, team);
            var states = await statesQ.ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                states[i].IsSolved = value;
            }
            await Context.SaveChangesAsync();
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
