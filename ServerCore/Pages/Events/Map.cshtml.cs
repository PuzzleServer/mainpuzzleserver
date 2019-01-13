using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class MapModel : EventSpecificPageModel
    {
        public List<PuzzleStats> Puzzles { get; private set; }

        public List<TeamStats> Teams { get; private set; }

        public StateStats[,] StateMap { get; private set; }

        public MapModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // get the puzzles and teams
            List<PuzzleStats> puzzles;

            if (EventRole == EventRole.admin)
            {
                puzzles = await _context.Puzzles.Where(p => p.Event == Event).Select(p => new PuzzleStats() { Puzzle = p }).ToListAsync();
            }
            else
            {
                puzzles = await UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser).Select(p => new PuzzleStats() { Puzzle = p }).ToListAsync();
            }

            List<TeamStats> teams = await _context.Teams.Where(t => t.Event == Event).Select(t => new TeamStats() { Team = t }).ToListAsync();

            // build an ID-based lookup for puzzles and teams
            Dictionary<int, PuzzleStats> puzzleLookup = new Dictionary<int, PuzzleStats>();
            puzzles.ForEach(p => puzzleLookup[p.Puzzle.ID] = p);

            Dictionary<int, TeamStats> teamLookup = new Dictionary<int, TeamStats>();
            teams.ForEach(t => teamLookup[t.Team.ID] = t);

            // tabulate solve counts and team scores
            List<PuzzleStatePerTeam> states = await PuzzleStateHelper.GetSparseQuery(_context, Event, null, null, EventRole == EventRole.admin ? null : LoggedInUser).ToListAsync();
            List<StateStats> stateList = new List<StateStats>(states.Count);
            foreach (PuzzleStatePerTeam state in states)
            {
                // TODO: Is it more performant to prefilter the states if an author, or is this sufficient?
                if (!puzzleLookup.TryGetValue(state.PuzzleID, out PuzzleStats puzzle) || !teamLookup.TryGetValue(state.TeamID, out TeamStats team))
                {
                    continue;
                }

                stateList.Add(new StateStats() { Puzzle = puzzle, Team = team, UnlockedTime = state.UnlockedTime, SolvedTime = state.SolvedTime });

                if (state.SolvedTime != null)
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

            Puzzles = puzzles;
            Teams = teams;
            StateMap = stateMap;

            return Page();
        }

        public class PuzzleStats
        {
            public Puzzle Puzzle { get; set; }
            public int SolveCount { get; set; }
            public int SortOrder { get; set; }
        }

        public class TeamStats
        {
            public Team Team { get; set; }
            public int SolveCount { get; set; }
            public int Score { get; set; }
            public int SortOrder { get; set; }
            public int? Rank { get; set; }
            public DateTime FinalMetaSolveTime { get; set; } = DateTime.MaxValue;
        }

        public class StateStats
        {
            public static StateStats Default { get; } = new StateStats();

            public PuzzleStats Puzzle { get; set; }
            public TeamStats Team { get; set; }
            public DateTime? UnlockedTime { get; set; }
            public DateTime? SolvedTime { get; set; }

            public string DisplayText
            {
                get
                {
                    return SolvedTime != null ? "C" : UnlockedTime != null ? "U" : "L";
                }
            }

            public int DisplayHue
            {
                get
                {
                    return SolvedTime != null ? 120 : UnlockedTime != null ? 60 : 0;
                }
            }

            public int DisplayLightness
            {
                get
                {
                    if (SolvedTime != null)
                    {
                        int minutes = (int)((DateTime.UtcNow - SolvedTime.Value).TotalMinutes);
                        return 75 - (Math.Min(minutes, 236) >> 2);
                    }
                    else if (UnlockedTime != null)
                    {
                        int minutes = (int)((DateTime.UtcNow - UnlockedTime.Value).TotalMinutes);
                        return 75 - (Math.Min(minutes, 236) >> 2);
                    }
                    else
                    {
                        return 100;
                    }
                }
            }
        }
    }
}
