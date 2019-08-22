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

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsRegisteredForEvent")]
    public class FastestSolvesModel : EventSpecificPageModel
    {
        public List<PuzzleStats> Puzzles { get; private set; }

        public PuzzleStateFilter? StateFilter { get; set; }

        public SortOrder? Sort { get; set; }

        public Team CurrentTeam { get; set; }

        private const SortOrder DefaultSort = SortOrder.RankAscending;

        public FastestSolvesModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task OnGetAsync(SortOrder? sort, PuzzleStateFilter? stateFilter)
        {
            this.Sort = sort;
            this.StateFilter = stateFilter;
            this.CurrentTeam = (await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser));

            Dictionary<int, string> teamNameLookup = new Dictionary<int, string>();

            // build an ID-to-name mapping to improve perf
            var names = await _context.Teams.Where(t => t.Event == Event)
                .Select(t => new { t.ID, t.Name })
                .ToListAsync();

            names.ForEach(t => teamNameLookup[t.ID] = t.Name);

            DateTime submissionEnd = Event.AnswerSubmissionEnd;
            // get the page data: puzzle, solve count, top three fastest
            var puzzlesData = await PuzzleStateHelper.GetSparseQuery(_context, this.Event, null, null)
                .Where(s => (s.SolvedTime != null &&
                             s.Puzzle.IsPuzzle &&
                             s.SolvedTime <= submissionEnd &&
                             s.Team.IsDisqualified == false))
                .GroupBy(state => state.Puzzle)
                .Select(g => new {
                    Puzzle = g.Key,
                    SolveCount = g.Count(),
                    Fastest = g.OrderBy(s => s.SolvedTime - s.UnlockedTime).Take(3).Select(s => new { s.Team.ID, Time = s.SolvedTime - s.UnlockedTime }),
                    IsSolvedByUserTeam = g.Where(s => s.Team == this.CurrentTeam).Any()
                })
                .OrderByDescending(p => p.SolveCount).ThenBy(p => p.Puzzle.Name)
                .ToListAsync();

            var unlockedData = this.CurrentTeam == null ? null : (new HashSet<int>(
                await PuzzleStateHelper.GetSparseQuery(_context, this.Event, null, null)
                    .Where(state => state.Team == this.CurrentTeam && state.UnlockedTime != null)
                    .Select(s => s.PuzzleID)
                    .ToListAsync()));

            var puzzles = new List<PuzzleStats>(puzzlesData.Count);
            for (int i = 0; i < puzzlesData.Count; i++)
            {
                var data = puzzlesData[i];

                // For players, we will hide puzzles they have not unlocked yet.
                if (EventRole == EventRole.play &&
                    (unlockedData == null ||
                     !unlockedData.Contains(data.Puzzle.ID)))
                {
                    continue;
                }

                var stats = new PuzzleStats()
                {
                    Puzzle = data.Puzzle,
                    SolveCount = data.SolveCount,
                    SortOrder = i,
                    Fastest = data.Fastest.Select(f => new FastRecord() { ID = f.ID, Name = teamNameLookup[f.ID], Time = f.Time }).ToArray(),
                    IsSolved = data.IsSolvedByUserTeam
                };

                puzzles.Add(stats);
            }

            if (this.StateFilter == PuzzleStateFilter.Unsolved)
            {
                puzzles = puzzles.Where(stats => !stats.IsSolved).ToList();
            }

            switch (sort ?? DefaultSort)
            {
                case SortOrder.RankAscending:
                    puzzles.Sort((rhs, lhs) => (rhs.SortOrder - lhs.SortOrder));
                    break;
                case SortOrder.RankDescending:
                    puzzles.Sort((rhs, lhs) => (lhs.SortOrder - rhs.SortOrder));
                    break;
                case SortOrder.CountAscending:
                    puzzles.Sort((rhs, lhs) => (rhs.SolveCount - lhs.SolveCount));
                    break;
                case SortOrder.CountDescending:
                    puzzles.Sort((rhs, lhs) => (lhs.SolveCount - rhs.SolveCount));
                    break;
                case SortOrder.PuzzleAscending:
                    puzzles.Sort((rhs, lhs) => (String.Compare(rhs.Puzzle.Name,
                                                               lhs.Puzzle.Name,
                                                               StringComparison.OrdinalIgnoreCase)));

                    break;
                case SortOrder.PuzzleDescending:
                    puzzles.Sort((rhs, lhs) => (String.Compare(lhs.Puzzle.Name,
                                                               rhs.Puzzle.Name,
                                                               StringComparison.OrdinalIgnoreCase)));

                    break;
                default:
                    throw new ArgumentException($"unknown sort: {sort}");
            }

            this.Puzzles = puzzles;
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

        public class PuzzleStats
        {
            public Puzzle Puzzle;
            public int SolveCount;
            public int SortOrder;
            public FastRecord[] Fastest;
            public bool IsSolved;
        }

        public class FastRecord
        {
            public int ID;
            public string Name;
            public TimeSpan? Time;
        }

        public enum SortOrder
        {
            RankAscending,
            RankDescending,
            PuzzleAscending,
            PuzzleDescending,
            CountAscending,
            CountDescending
        }

        public enum PuzzleStateFilter
        {
            All,
            Unsolved
        }
    }
}
