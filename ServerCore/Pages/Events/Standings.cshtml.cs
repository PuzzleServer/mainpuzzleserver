using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class StandingsModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public List<TeamStats> Teams { get; private set; }

        public StandingsModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        // TODO: This bears striking similarities to the puzzle state map and maybe key logic could be shared. But I am also trying to keep things efficient, so maybe not.
        // This one might also be writable in a far shorter query by someone who knows what they are doing (not me).
        public async Task OnGetAsync()
        {
            // get the puzzles and teams
            var puzzles = await _context.Puzzles.Where(p => p.Event == this.Event).ToListAsync();
            var teams = await _context.Teams.Where(t => t.Event == this.Event).Select(t => new TeamStats() { Team = t }).ToListAsync();

            // build an ID-based lookup for teams
            var puzzleLookup = new Dictionary<int, Puzzle>();
            puzzles.ForEach(p => puzzleLookup[p.ID] = p);

            var teamLookup = new Dictionary<int, TeamStats>();
            teams.ForEach(t => teamLookup[t.Team.ID] = t);

            // tabulate team scores. Unlike the state map, prefilter to solves only as that's all we need.
            var states = await PuzzleStateHelper.GetSparseQuery(_context, this.Event, null, null).Where(s => s.SolvedTime != null).ToListAsync();
            foreach (var state in states)
            {
                if (!puzzleLookup.TryGetValue(state.PuzzleID, out Puzzle puzzle) || !teamLookup.TryGetValue(state.TeamID, out TeamStats team))
                {
                    continue;
                }

                team.SolveCount++;
                team.Score += puzzle.SolveValue;

                if (puzzle.IsFinalPuzzle)
                {
                    team.FinalMetaSolveTime = state.SolvedTime.Value;
                }
            }

            // sort teams by metameta/score/solves, add the sort index to the lookup
            teams = teams.OrderBy(t => t.FinalMetaSolveTime).ThenByDescending(t => t.Score).ThenByDescending(t => t.SolveCount).ThenBy(t => t.Team.Name).ToList();
            for (int i = 0; i < teams.Count; i++)
            {
                teams[i].SortOrder = i;
            }

            this.Teams = teams;
        }

        public class TeamStats
        {
            public Team Team;
            public int SolveCount;
            public int Score;
            public int SortOrder;
            public DateTime FinalMetaSolveTime = DateTime.MaxValue;
        }
    }
}
