using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class PlayModel : EventSpecificPageModel
    {
        // see https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-2.1 to make this sortable!

        private readonly ServerCore.Models.PuzzleServerContext _context;

        public PlayModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<PuzzleWithState> PuzzlesWithState { get;set; }

        public int TeamID { get; set; }

        public string Sort { get; set; }

        private const string DefaultSort = "puzzle";

        public async Task OnGetAsync(int id, string sort)
        {
            this.TeamID = id;
            this.Sort = sort;

            // all puzzles for this event that are real puzzles
            var puzzlesInEventQ = _context.Puzzles.Where(puzzle => puzzle.Event.ID == this.Event.ID && puzzle.IsPuzzle);

            // all puzzle states for this team that are unlocked (note: IsUnlocked bool is going to harm perf, just null check the time here)
            var stateForTeamQ = _context.PuzzleStatePerTeam.Where(state => state.TeamID == id && state.UnlockedTime != null);

            // join 'em (note: just getting all properties for max flexibility, can pick and choose columns for perf later)
            var visiblePuzzlesQ = puzzlesInEventQ.Join(stateForTeamQ, (puzzle => puzzle.ID), (state => state.PuzzleID), (puzzle, state) => new PuzzleWithState(puzzle, state));

            switch (sort ?? DefaultSort)
            {
                case "puzzle":
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(puzzleWithState => puzzleWithState.Puzzle.Name);
                    break;
                case "puzzle_desc":
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(puzzleWithState => puzzleWithState.Puzzle.Name);
                    break;
                case "group":
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(puzzleWithState => puzzleWithState.Puzzle.Group).ThenBy(puzzleWithState => puzzleWithState.Puzzle.OrderInGroup);
                    break;
                case "group_desc":
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(puzzleWithState => puzzleWithState.Puzzle.Group).ThenByDescending(puzzleWithState => puzzleWithState.Puzzle.OrderInGroup);
                    break;
                case "solve":
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(puzzleWithState => puzzleWithState.State.SolvedTime ?? DateTime.MaxValue);
                    break;
                case "solve_desc":
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(puzzleWithState => puzzleWithState.State.SolvedTime ?? DateTime.MaxValue);
                    break;
                default:
                    throw new ArgumentException($"unknown sort: {sort}");
            }

            PuzzlesWithState = await visiblePuzzlesQ.ToListAsync();
        }

        public string SortForColumnLink(string column)
        {
            string result = column;

            if (result == (this.Sort ?? DefaultSort))
            {
                result += "_desc";
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
    }
}
