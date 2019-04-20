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

            var teamsData = await PuzzleStateHelper.GetSparseQuery(_context, this.Event, null, null)
                .Where(s => s.SolvedTime != null && s.Puzzle.IsPuzzle)
                .GroupBy(state => state.Team)
                .Select(g => new {
                    Team = g.Key,
                    SolveCount = g.Count(),
                    Score = g.Sum(s => s.Puzzle.SolveValue),
                    FinalMetaSolveTime = g.Where(s => s.Puzzle.IsCheatCode).Any() ?
                        DateTime.MaxValue :
                        (g.Where(s => s.Puzzle.IsFinalPuzzle).Select(s => s.SolvedTime).FirstOrDefault() ?? DateTime.MaxValue)
                })
                .OrderBy(t => t.FinalMetaSolveTime).ThenByDescending(t => t.Score).ThenBy(t => t.Team.Name)
                .ToListAsync();

            var teams = new List<TeamStats>(teamsData.Count);
            TeamStats prevStats = null;
            for (int i = 0; i < teamsData.Count; i++)
            {
                var data = teamsData[i];
                var stats = new TeamStats()
                {
                    Team = data.Team,
                    SolveCount = data.SolveCount,
                    Score = data.Score,
                    FinalMetaSolveTime = data.FinalMetaSolveTime
                };

                if (prevStats == null || stats.FinalMetaSolveTime != prevStats.FinalMetaSolveTime || stats.Score != prevStats.Score)
                {
                    stats.Rank = i + 1;
                }
                else
                {
                    stats.Rank = prevStats.Rank;
                }

                teams.Add(stats);
                prevStats = stats;
            }

            switch(sort)
            {
                case SortOrder.RankAscending:
                    break;
                case SortOrder.RankDescending:
                    teams.Reverse();
                    break;
                case SortOrder.NameAscending:
                    teams.Sort((a, b) => a.Team.Name.CompareTo(b.Team.Name));
                    break;
                case SortOrder.NameDescending:
                    teams.Sort((a, b) => -a.Team.Name.CompareTo(b.Team.Name));
                    break;
                case SortOrder.PuzzlesAscending:
                    teams.Sort((a, b) => a.SolveCount.CompareTo(b.SolveCount));
                    break;
                case SortOrder.PuzzlesDescending:
                    teams.Sort((a, b) => -a.SolveCount.CompareTo(b.SolveCount));
                    break;
                case SortOrder.ScoreAscending:
                    teams.Sort((a, b) => a.Score.CompareTo(b.Score));
                    break;
                case SortOrder.ScoreDescending:
                    teams.Sort((a, b) => -a.Score.CompareTo(b.Score));
                    break;
                case SortOrder.HintsUsedAscending:
                    teams.Sort((a, b) => a.Score.CompareTo(b.Team.HintCoinsUsed));
                    break;
                case SortOrder.HintsUsedDescending:
                    teams.Sort((a, b) => -a.Score.CompareTo(b.Team.HintCoinsUsed));
                    break;
            }

            this.Teams = teams;
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
