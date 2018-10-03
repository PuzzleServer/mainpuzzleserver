using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class MapModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public List<PuzzleStats> Puzzles { get; private set; }

        public List<TeamStats> Teams { get; private set; }

        public StateStats[,] StateMap { get; private set; }

        public MapModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // get the puzzles and teams
            // TODO: Filter puzzles if an author; no need to filter teams. Revisit when authors exist.
            var puzzles = await _context.Puzzles.Where(p => p.Event == this.Event).Select(p => new PuzzleStats() { Puzzle = p }).ToListAsync();
            var teams = await _context.Teams.Where(t => t.Event == this.Event).Select(t => new TeamStats() { Team = t }).ToListAsync();

            // build an ID-based lookup for puzzles and teams
            var puzzleLookup = new Dictionary<int, PuzzleStats>();
            puzzles.ForEach(p => puzzleLookup[p.Puzzle.ID] = p);

            var teamLookup = new Dictionary<int, TeamStats>();
            teams.ForEach(t => teamLookup[t.Team.ID] = t);

            // tabulate solve counts and team scores
            var states = await PuzzleStateHelper.GetSparseQuery(_context, this.Event, null, null).ToListAsync();
            var stateList = new List<StateStats>(states.Count);
            foreach (var state in states)
            {
                // TODO: Is it more performant to prefilter the states if an author, or is this sufficient?
                if (!puzzleLookup.TryGetValue(state.PuzzleID, out PuzzleStats puzzle) || !teamLookup.TryGetValue(state.TeamID, out TeamStats team))
                {
                    continue;
                }

                stateList.Add(new StateStats() { Puzzle = puzzle, Team = team, UnlockedTime = state.UnlockedTime, SolvedTime = state.SolvedTime });

                if (state.IsSolved)
                {
                    puzzle.SolveCount++;
                    team.SolveCount++;
                    team.Score += puzzle.Puzzle.SolveValue;

                    if (puzzle.Puzzle.IsFinalPuzzle)
                    {
                        team.FinalMetaSolveTime = state.SolvedTime.Value;
                    }
                }
            }

            // sort puzzles by solve count, add the sort index to the lookup
            puzzles = puzzles.OrderByDescending(p => p.SolveCount).ThenBy(p => p.Puzzle.Name).ToList();
            for (int i = 0; i < puzzles.Count; i++)
            {
                puzzles[i].SortOrder = i;
            }

            // sort teams by metameta/score, add the sort index to the lookup
            teams = teams.OrderBy(t => t.FinalMetaSolveTime).ThenByDescending(t => t.Score).ThenBy(t => t.Team.Name).ToList();
            for (int i = 0; i < teams.Count; i++)
            {
                if (i == 0 || teams[i].FinalMetaSolveTime != teams[i - 1].FinalMetaSolveTime || teams[i].Score != teams[i - 1].Score)
                {
                    teams[i].Rank = i + 1;
                }

                teams[i].SortOrder = i;
            }

            // Build the map
            var stateMap = new StateStats[puzzles.Count, teams.Count];
            stateList.ForEach(state => stateMap[state.Puzzle.SortOrder, state.Team.SortOrder] = state);

            this.Puzzles = puzzles;
            this.Teams = teams;
            this.StateMap = stateMap;
        }

        public class PuzzleStats
        {
            public Puzzle Puzzle;
            public int SolveCount;
            public int SortOrder;
        }

        public class TeamStats
        {
            public Team Team;
            public int SolveCount;
            public int Score;
            public int SortOrder;
            public int? Rank;
            public DateTime FinalMetaSolveTime = DateTime.MaxValue;
        }

        public class StateStats
        {
            public static StateStats Default = new StateStats();

            public PuzzleStats Puzzle;
            public TeamStats Team;
            public DateTime? UnlockedTime;
            public DateTime? SolvedTime;
        }
    }
}
