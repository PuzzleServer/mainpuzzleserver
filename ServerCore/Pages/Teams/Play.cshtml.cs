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
    [Authorize(Policy = "IsPlayer")]
    public class PlayModel : EventSpecificPageModel
    {
        // see https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-2.1 to make this sortable!

        public PlayModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<PuzzleWithState> PuzzlesWithState { get; set; }

        public int TeamID { get; set; }

        public SortOrder? Sort { get; set; }

        private const SortOrder DefaultSort = SortOrder.PuzzleAscending;

        public async Task OnGetAsync(SortOrder? sort)
        {
            Team myTeam = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            if (myTeam != null)
            {
                this.TeamID = myTeam.ID;
            }
            else
            {
                throw new Exception("Not currently registered for a team");
            }
            this.Sort = sort;

            // all puzzles for this event that are real puzzles
            var puzzlesInEventQ = _context.Puzzles.Where(puzzle => puzzle.Event.ID == this.Event.ID && puzzle.IsPuzzle);

            // all puzzle states for this team that are unlocked (note: IsUnlocked bool is going to harm perf, just null check the time here)
            // Note that it's OK if some puzzles do not yet have a state record; those puzzles are clearly still locked and hence invisible.
            var stateForTeamQ = _context.PuzzleStatePerTeam.Where(state => state.TeamID == this.TeamID && state.UnlockedTime != null);

            // join 'em (note: just getting all properties for max flexibility, can pick and choose columns for perf later)
            // Note: EF gotcha is that you have to join into anonymous types in order to not lose valuable stuff
            var visiblePuzzlesQ = puzzlesInEventQ.Join(stateForTeamQ, (puzzle => puzzle.ID), (state => state.PuzzleID), (Puzzle, State) => new { Puzzle, State });

            switch (sort ?? DefaultSort)
            {
                case SortOrder.PuzzleAscending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(puzzleWithState => puzzleWithState.Puzzle.Name);
                    break;
                case SortOrder.PuzzleDescending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(puzzleWithState => puzzleWithState.Puzzle.Name);
                    break;
                case SortOrder.GroupAscending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(puzzleWithState => puzzleWithState.Puzzle.Group).ThenBy(puzzleWithState => puzzleWithState.Puzzle.OrderInGroup);
                    break;
                case SortOrder.GroupDescending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(puzzleWithState => puzzleWithState.Puzzle.Group).ThenByDescending(puzzleWithState => puzzleWithState.Puzzle.OrderInGroup);
                    break;
                case SortOrder.SolveAscending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(puzzleWithState => puzzleWithState.State.SolvedTime ?? DateTime.MaxValue);
                    break;
                case SortOrder.SolveDescending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(puzzleWithState => puzzleWithState.State.SolvedTime ?? DateTime.MaxValue);
                    break;
                default:
                    throw new ArgumentException($"unknown sort: {sort}");
            }

            PuzzlesWithState = (await visiblePuzzlesQ.ToListAsync()).Select(x => new PuzzleWithState(x.Puzzle, x.State)).ToList();
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

        public class PuzzleWithState
        {
            public Puzzle Puzzle { get; }
            public PuzzleStatePerTeam State { get; }

            public PuzzleWithState(Puzzle puzzle, PuzzleStatePerTeam state)
            {
                this.Puzzle = puzzle;
                this.State = state;
            }
        }

        public enum SortOrder
        {
            PuzzleAscending,
            PuzzleDescending,
            GroupAscending,
            GroupDescending,
            SolveAscending,
            SolveDescending
        }
    }
}
