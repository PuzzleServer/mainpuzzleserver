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

        public string Groups { get; set; }

        public MapModel(PuzzleServerContext serverContext,
                        UserManager<IdentityUser> userManager)
            : base(serverContext, userManager) { }

        public async Task<IActionResult> OnGetAsync(int? refresh, string? groups)
        {
            if (refresh != null)
            {
                Refresh = refresh;
            }

            Groups = groups ?? "Meta|*";

            Dictionary<string, int> groupSortIndex = new Dictionary<string, int>();
            string[] groupList = Groups.Split("|");
            for (int i = 0; i < groupList.Length; i++)
            {
                groupSortIndex[groupList[i]] = i;
            }

            // get the puzzles and teams
            List<PuzzleStats> puzzles;

            if (EventRole == EventRole.admin)
            {
                puzzles = await _context.Puzzles.Where(p => p.Event == Event)
                    .Select(p => new PuzzleStats() { Puzzle = p })
                    .ToListAsync();
            }
            else
            {
                puzzles = await UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                    .Select(p => new PuzzleStats() { Puzzle = p })
                    .ToListAsync();
            }

            List<TeamStats> teams = await _context.Teams.Where(t => t.Event == Event)
                .Select(t => new TeamStats() { Team = t })
                .ToListAsync();

            // build an ID-based lookup for puzzles and teams
            Dictionary<int, PuzzleStats> puzzleLookup = new Dictionary<int, PuzzleStats>();
            puzzles.ForEach(p => puzzleLookup[p.Puzzle.ID] = p);

            Dictionary<int, TeamStats> teamLookup = new Dictionary<int, TeamStats>();
            teams.ForEach(t => teamLookup[t.Team.ID] = t);

            // tabulate solve counts and team scores
            List<PuzzleStatePerTeam> states = await PuzzleStateHelper.GetSparseQuery(
                _context,
                Event,
                null,
                null,
                EventRole == EventRole.admin ? null : LoggedInUser).ToListAsync();

            // Get the earliest unlock time
            DateTime? earliestUnlock = null;
            foreach (PuzzleStatePerTeam state in states)
            {
                if (state.UnlockedTime != null)
                {
                    if (earliestUnlock == null || earliestUnlock.Value >= state.UnlockedTime.Value)
                    {
                        earliestUnlock = state.UnlockedTime;
                    }
                }
            }

                List<StateStats> stateList = new List<StateStats>(states.Count);
            foreach (PuzzleStatePerTeam state in states)
            {
                // TODO: Is it more performant to prefilter the states if an author, or is this sufficient?
                if (!puzzleLookup.TryGetValue(state.PuzzleID, out PuzzleStats puzzle) ||
                    !teamLookup.TryGetValue(state.TeamID, out TeamStats team))
                {
                    continue;
                }

                stateList.Add(new StateStats() {
                    Puzzle = puzzle,
                    Team = team,
                    UnlockedAtStart = state.UnlockedTime == earliestUnlock,
                    UnlockedTime = state.UnlockedTime,
                    SolvedTime = state.SolvedTime,
                    LockedOut = state.IsEmailOnlyMode
                });

                if (state.SolvedTime != null)
                {
                    puzzle.SolveCount++;
                    team.SolveCount++;
                    team.Score += puzzle.Puzzle.SolveValue;

                    if (puzzle.Puzzle.IsCheatCode)
                    {
                        team.CheatCodeUsed = true;
                        team.FinalMetaSolveTime = DateTime.MaxValue;
                    }

                    if (puzzle.Puzzle.IsFinalPuzzle && !team.CheatCodeUsed)
                    {
                        team.FinalMetaSolveTime = state.SolvedTime.Value;
                    }
                }
            }

            // default: sort puzzles by group, then solve count, add the sort index to the lookup
            // but put non-puzzles to the end
            // however, any explicitly-requested group ordering wins
            for (int i = 0; i < puzzles.Count; i++)
            {
                int groupOrder;

                if (string.IsNullOrEmpty(puzzles[i].Puzzle.Group) || !groupSortIndex.TryGetValue(puzzles[i].Puzzle.Group, out groupOrder))
                {
                    if (!groupSortIndex.TryGetValue("*", out groupOrder))
                    {
                        groupOrder = groupSortIndex.Count;
                    }
                }
                puzzles[i].GroupOrder = groupOrder;
            }

            puzzles = puzzles.OrderBy(p => p.GroupOrder)
                .ThenByDescending(p => p.Puzzle.IsPuzzle)
                .ThenBy(p => p.Puzzle.Group)
                .ThenBy(p => p.SolveCount)
                .ThenBy(p => p.Puzzle.Name)
                .ToList();

            for (int i = 0; i < puzzles.Count; i++)
            {
                puzzles[i].SortOrder = i;
            }

            // sort teams by metameta/score, add the sort index to the lookup
            teams = teams.OrderBy(t => t.FinalMetaSolveTime)
                .ThenByDescending(t => t.Score)
                .ThenBy(t => t.Team.Name)
                .ToList();

            for (int i = 0; i < teams.Count; i++)
            {
                if (i == 0 ||
                    teams[i].FinalMetaSolveTime != teams[i - 1].FinalMetaSolveTime ||
                    teams[i].Score != teams[i - 1].Score)
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
            public int GroupOrder { get; set; }
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
            public bool CheatCodeUsed { get; set; }
            public DateTime FinalMetaSolveTime { get; set; } = DateTime.MaxValue;
        }

        public class StateStats
        {
            public static StateStats Default { get; } = new StateStats();

            public PuzzleStats Puzzle { get; set; }
            public TeamStats Team { get; set; }
            public bool UnlockedAtStart { get; set; }
            public DateTime? UnlockedTime { get; set; }
            public DateTime? SolvedTime { get; set; }
            public Boolean LockedOut { get; set; }

            public string Classes
            {
                get
                {
                    string puzzleType = null;
                    string puzzleState = null;

                    if (Puzzle != null && Puzzle.Puzzle.IsFinalPuzzle)
                    {
                        puzzleType = "final-puzzle";
                    }
                    else if (Puzzle != null && Puzzle.Puzzle.IsMetaPuzzle)
                    {
                        puzzleType = "meta-puzzle";
                    }

                    if (LockedOut)
                    {
                        puzzleState = "email-mode";
                    }
                    else if (SolvedTime != null)
                    {
                        int minutes = (int)((DateTime.UtcNow - SolvedTime.Value).TotalMinutes);
                        puzzleState = minutes > 15 ? "solved-old" : "solved-recent";
                    }
                    else if (!UnlockedAtStart)
                    {
                        if (UnlockedTime == null)
                        {
                            puzzleState = "still-locked";
                        }
                        else
                        {
                            int minutes = (int)((DateTime.UtcNow - UnlockedTime.Value).TotalMinutes);
                            puzzleState = minutes > 15 ? "unlocked-old" : "unlocked-recent";
                        }
                    }

                    return puzzleState != null ? $"statecell {puzzleType} {puzzleState}" : $"statecell {puzzleType}";
                }
            }
        }
    }
}
