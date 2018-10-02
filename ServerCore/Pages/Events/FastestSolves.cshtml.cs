using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;
using ServerCore.Models;

namespace ServerCore.Pages.Events
{
    public class FastestSolvesModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public List<PuzzleStats> Puzzles { get; private set; }

        public FastestSolvesModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        // TODO: This bears striking similarities to the puzzle state map and maybe key logic could be shared. But I am also trying to keep things efficient, so maybe not.
        // This one might also be writable in a far shorter query by someone who knows what they are doing (not me).
        public async Task OnGetAsync()
        {
            // get the puzzles and teams
            // Unlike other cases, filter to just real puzzles
            var puzzles = await _context.Puzzles.Where(p => p.Event == this.Event && p.IsPuzzle).Select(p => new PuzzleStats() { Puzzle = p }).ToListAsync();
            var teams = await _context.Teams.Where(t => t.Event == this.Event).ToListAsync();

            // build an ID-based lookup for puzzles and teams
            var puzzleLookup = new Dictionary<int, PuzzleStats>();
            puzzles.ForEach(p => puzzleLookup[p.Puzzle.ID] = p);

            var teamLookup = new Dictionary<int, Team>();
            teams.ForEach(t => teamLookup[t.ID] = t);

            // tabulate solve counts and fastest solves. Unlike the state map, prefilter to solves only as that's all we need.
            var states = await PuzzleStateHelper.GetSparseQuery(_context, this.Event, null, null).Where(s => s.SolvedTime != null).ToListAsync();
            foreach (var state in states)
            {
                // TODO: Is it more performant to prefilter the states if an author, or is this sufficient?
                if (!puzzleLookup.TryGetValue(state.PuzzleID, out PuzzleStats puzzle) || !teamLookup.TryGetValue(state.TeamID, out Team team))
                {
                    continue;
                }

                puzzle.SolveCount++;

                if (state.IsUnlocked)
                {
                    TimeSpan solveTime = state.SolvedTime.Value - state.UnlockedTime.Value;
                    if (puzzle.FastestSolve == null || solveTime < puzzle.FastestSolve)
                    {
                        puzzle.FastestSolve = solveTime;
                        puzzle.FastestTeam = team.Name;
                    }
                }
            }

            // sort puzzles by solve count, add the sort index to the lookup
            puzzles = puzzles.OrderByDescending(p => p.SolveCount).ThenBy(p => p.Puzzle.Name).ToList();
            for (int i = 0; i < puzzles.Count; i++)
            {
                puzzles[i].SortOrder = i;
            }

            this.Puzzles = puzzles;
        }

        public class PuzzleStats
        {
            public Puzzle Puzzle;
            public int SolveCount;
            public int SortOrder;
            public string FastestTeam;
            public TimeSpan? FastestSolve;
        }
    }
}
