using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsRegisteredForEvent")]
    public class StandingsModel : EventSpecificPageModel
    {
        public List<TeamStats> Teams { get; private set; }

        public SortOrder? Sort { get; set; }

        private const SortOrder DefaultSort = SortOrder.RankAscending;

        public StandingsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task OnGetAsync(SortOrder? sort)
        {
            Sort = sort;

            var puzzleData = await _context.Puzzles
                .Where(p => p.Event == Event && p.IsPuzzle)
                .ToDictionaryAsync(p => p.ID, p => new { p.SolveValue, p.IsCheatCode, p.IsFinalPuzzle });

            var stateData = await PuzzleStateHelper.GetSparseQuery(_context, this.Event, null, null)
                .Where(pspt => pspt.SolvedTime != null)
                .Select(pspt => new { pspt.PuzzleID, pspt.TeamID, pspt.SolvedTime })
                .ToListAsync();

            // Hide disqualified teams from the standings page.
            var teams = await _context.Teams
                .Where(t => t.Event == Event && t.IsDisqualified == false)
                .ToListAsync();

            Dictionary<int, TeamStats> teamStats = new Dictionary<int, TeamStats>(teams.Count);
            foreach (var t in teams)
            {
                teamStats[t.ID] = new TeamStats { Team = t, FinalMetaSolveTime = DateTime.MaxValue };
            }

            foreach (var s in stateData)
            {
                if (!puzzleData.TryGetValue(s.PuzzleID, out var p) || !teamStats.TryGetValue(s.TeamID, out var ts))
                {
                    continue;
                }

                ts.Score += p.SolveValue;
                ts.SolveCount++;
                if (p.IsCheatCode)
                {
                    ts.Cheated = true;
                    ts.FinalMetaSolveTime = DateTime.MaxValue;
                }
                if (p.IsFinalPuzzle && !ts.Cheated)
                {
                    ts.FinalMetaSolveTime = s.SolvedTime.Value;
                }
            }

            var teamsFinal = teamStats.Values.OrderBy(t => t.FinalMetaSolveTime).ThenByDescending(t => t.Score).ThenBy(t => t.Team.Name).ToList();

            TeamStats prevStats = null;
            for (int i = 0; i < teamsFinal.Count; i++)
            {
                var stats = teamsFinal[i];

                if (prevStats == null || stats.FinalMetaSolveTime != prevStats.FinalMetaSolveTime || stats.Score != prevStats.Score)
                {
                    stats.Rank = i + 1;
                }
                else
                {
                    stats.Rank = prevStats.Rank;
                }

                prevStats = stats;
            }

            switch(sort)
            {
                case SortOrder.RankAscending:
                    break;
                case SortOrder.RankDescending:
                    teamsFinal.Reverse();
                    break;
                case SortOrder.NameAscending:
                    teamsFinal = teamsFinal.OrderBy(ts => ts.Team.Name).ToList();
                    break;
                case SortOrder.NameDescending:
                    teamsFinal = teamsFinal.OrderByDescending(ts => ts.Team.Name).ToList();
                    break;
                case SortOrder.PuzzlesAscending:
                    teamsFinal = teamsFinal.OrderBy(ts => ts.SolveCount).ThenBy(ts => ts.Rank).ThenBy(ts => ts.Team.Name).ToList();
                    break;
                case SortOrder.PuzzlesDescending:
                    teamsFinal = teamsFinal.OrderByDescending(ts => ts.SolveCount).ThenByDescending(ts => ts.Rank).ThenByDescending(ts => ts.Team.Name).ToList();
                    break;
                case SortOrder.ScoreAscending:
                    teamsFinal = teamsFinal.OrderBy(ts => ts.Score).ThenBy(ts => ts.Rank).ThenBy(ts => ts.Team.Name).ToList();
                    break;
                case SortOrder.ScoreDescending:
                    teamsFinal = teamsFinal.OrderByDescending(ts => ts.Score).ThenByDescending(ts => ts.Rank).ThenByDescending(ts => ts.Team.Name).ToList();
                    break;
                case SortOrder.HintsUsedAscending:
                    teamsFinal = teamsFinal.OrderBy(ts => ts.Team.HintCoinsUsed).ThenBy(ts => ts.Rank).ThenBy(ts => ts.Team.Name).ToList();
                    break;
                case SortOrder.HintsUsedDescending:
                    teamsFinal = teamsFinal.OrderByDescending(ts => ts.Team.HintCoinsUsed).ThenByDescending(ts => ts.Rank).ThenByDescending(ts => ts.Team.Name).ToList();
                    break;
            }

            this.Teams = teamsFinal;
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

        public class TeamStats
        {
            public Team Team;
            public bool Cheated;
            public int SolveCount;
            public int Score;
            public int? Rank;
            public DateTime FinalMetaSolveTime = DateTime.MaxValue;
        }

        public enum SortOrder
        {
            RankAscending,
            RankDescending,
            NameAscending,
            NameDescending,
            PuzzlesAscending,
            PuzzlesDescending,
            ScoreAscending,
            ScoreDescending,
            HintsUsedAscending,
            HintsUsedDescending
        }
    }
}
