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

            // Get the page data: puzzle, solve count, top three fastest
            // Puzzle solve counts
            var solveCounts = await (from pspt in _context.PuzzleStatePerTeam
                                     where pspt.SolvedTime != null &&
                                     pspt.Puzzle.IsPuzzle &&
                                     pspt.UnlockedTime != null &&
                                     pspt.SolvedTime <= submissionEnd &&
                                     !pspt.Team.IsDisqualified &&
                                     pspt.Puzzle.Event == Event
                                     let puzzleToGroup = new { PuzzleID = pspt.Puzzle.ID, PuzzleName = pspt.Puzzle.Name } // Using 'let' to work around EF grouping limitations (https://www.codeproject.com/Questions/5266406/Invalidoperationexception-the-LINQ-expression-for)
                                     group puzzleToGroup by puzzleToGroup.PuzzleID into puzzleGroup
                                     orderby puzzleGroup.Count() descending, puzzleGroup.Max(puzzleGroup => puzzleGroup.PuzzleName) // Using Max(PuzzleName) because only aggregate operators are allowed
                                     select new { PuzzleID = puzzleGroup.Key, PuzzleName = puzzleGroup.Max(puzzleGroup => puzzleGroup.PuzzleName), SolveCount = puzzleGroup.Count() }).ToListAsync();

            // Getting the top 3 requires working around EF limitations translating sorting results within a group to SQL. Workaround from https://github.com/dotnet/efcore/issues/13805

            // Filter to solved puzzles
            var psptToQuery = _context.PuzzleStatePerTeam.Where(pspt => pspt.Puzzle.EventID == Event.ID &&
            pspt.Puzzle.IsPuzzle &&
            pspt.UnlockedTime != null &&
            pspt.SolvedTime != null &&
            pspt.SolvedTime < submissionEnd &&
            !pspt.Team.IsDisqualified
            );

            // Sort by time and get the top 3
            var fastestResults = _context.PuzzleStatePerTeam
                .Select(pspt => pspt.PuzzleID).Distinct()
                .SelectMany(puzzleId => psptToQuery
                    .Where(pspt => pspt.PuzzleID == puzzleId)
                    .OrderBy(pspt => EF.Functions.DateDiffSecond(pspt.UnlockedTime, pspt.SolvedTime))
                    .Take(3), (puzzleId, pspt) => pspt)
                .ToLookup(pspt => pspt.PuzzleID, pspt => new { pspt.TeamID, pspt.SolvedTime, pspt.UnlockedTime });

            var unlockedData = this.CurrentTeam == null ? null : (
                await PuzzleStateHelper.GetSparseQuery(_context, this.Event, null, null)
                    .Where(state => state.Team == this.CurrentTeam && state.UnlockedTime != null)
                    .Select(s => new { s.PuzzleID, IsSolvedByUserTeam = (s.SolvedTime < submissionEnd) })
                    .ToDictionaryAsync(s => s.PuzzleID));

            var puzzles = new List<PuzzleStats>(solveCounts.Count);
            for (int i = 0; i < solveCounts.Count; i++)
            {
                var data = solveCounts[i];

                // For players, we will hide puzzles they have not unlocked yet.
                if (EventRole == EventRole.play &&
                    (unlockedData == null ||
                     !unlockedData.ContainsKey(data.PuzzleID)))
                {
                    continue;
                }

                var stats = new PuzzleStats()
                {
                    PuzzleName = data.PuzzleName,
                    SolveCount = data.SolveCount,
                    SortOrder = i,
                    Fastest = fastestResults[data.PuzzleID].Select(f => new FastRecord() { ID = f.TeamID, Name = teamNameLookup[f.TeamID], Time = f.SolvedTime - f.UnlockedTime }).ToArray(),
                    IsSolved = unlockedData[data.PuzzleID].IsSolvedByUserTeam
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
                    puzzles.Sort((rhs, lhs) => (String.Compare(rhs.PuzzleName,
                                                               lhs.PuzzleName,
                                                               StringComparison.OrdinalIgnoreCase)));

                    break;
                case SortOrder.PuzzleDescending:
                    puzzles.Sort((rhs, lhs) => (String.Compare(lhs.PuzzleName,
                                                               rhs.PuzzleName,
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
            public string PuzzleName;
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
