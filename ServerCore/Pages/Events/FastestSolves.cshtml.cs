using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class FastestSolvesModel : EventSpecificPageModel
    {
        public string TeamSectionNotShowMessage { get; private set; }

        public string SinglePlayerPuzzleSectionNotShowMessage { get; private set; }

        public List<TeamPuzzleStats> TeamPuzzles { get; private set; }

        public List<SinglePlayerPuzzleStats> SinglePlayerPuzzles { get; private set; }

        public PuzzleStateFilter? StateFilter { get; set; }

        public TeamPuzzleSortOrder? TeamPuzzleSort { get; set; }

        public SinglePlayerPuzzleSortOrder? SinglePlayerPuzzleSort { get; set; }

        public Team CurrentTeam { get; set; }

        private const TeamPuzzleSortOrder TeamPuzzleDefaultSort = TeamPuzzleSortOrder.RankAscending;

        private const SinglePlayerPuzzleSortOrder SinglePlayerPuzzleDefaultSort = SinglePlayerPuzzleSortOrder.CountDescending;

        public FastestSolvesModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task OnGetAsync(TeamPuzzleSortOrder? teamPuzzleSort, SinglePlayerPuzzleSortOrder? singlePlayerPuzzleSort, PuzzleStateFilter? stateFilter)
        {
            this.TeamSectionNotShowMessage = string.Empty;
            this.SinglePlayerPuzzleSectionNotShowMessage = string.Empty;
            this.TeamPuzzleSort = teamPuzzleSort;
            this.SinglePlayerPuzzleSort = singlePlayerPuzzleSort;
            this.StateFilter = stateFilter;
            this.CurrentTeam = (await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser));

            HashSet<int> authorPuzzleIds = this.GetAuthorPuzzleIds();
            await this.updateTeamPuzzleStats(authorPuzzleIds);
            await this.updateSinglePlayerPuzzleStats(authorPuzzleIds);
        }

        private async Task updateTeamPuzzleStats(HashSet<int> authorPuzzleIds)
        {
            bool isRegisteredUser = await this.IsRegisteredUser();
            if (EventRole == EventRole.play && !isRegisteredUser)
            {
                this.TeamSectionNotShowMessage = "Please register for the event to see team standings.";
                this.TeamPuzzles = new List<TeamPuzzleStats>();
                return;
            }

            Dictionary<int, string> teamNameLookup = new Dictionary<int, string>();

            // build an ID-to-name mapping to improve perf
            var names = await _context.Teams.Where(t => t.Event == Event)
                .Select(t => new { t.ID, t.Name })
                .ToListAsync();

            names.ForEach(t => teamNameLookup[t.ID] = t.Name);

            DateTime submissionEnd = Event.AnswerSubmissionEnd;

            // Get the page data: puzzle, solve count, top three fastest
            // Puzzle solve counts
            var solveCounts = await(from pspt in _context.PuzzleStatePerTeam
                                    where pspt.SolvedTime != null &&
                                    pspt.Puzzle.IsPuzzle &&
                                    pspt.UnlockedTime != null &&
                                    pspt.SolvedTime <= submissionEnd &&
                                    !pspt.Team.IsDisqualified &&
                                    pspt.Puzzle.Event == Event &&
                                    (EventRole != EventRole.author || authorPuzzleIds.Contains(pspt.PuzzleID))
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
                !pspt.Team.IsDisqualified &&
                (EventRole != EventRole.author || authorPuzzleIds.Contains(pspt.PuzzleID)));

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

            var puzzles = new List<TeamPuzzleStats>(solveCounts.Count);
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

                FastRecord[] fastest;
                if (fastestResults.Contains(data.PuzzleID))
                {
                    fastest = fastestResults[data.PuzzleID].Select(f => new FastRecord() { ID = f.TeamID, Name = teamNameLookup[f.TeamID], Time = f.SolvedTime - f.UnlockedTime }).ToArray();
                }
                else
                {
                    fastest = Array.Empty<FastRecord>();
                }

                bool isSolved = false;
                if (unlockedData != null)
                {
                    isSolved = unlockedData[data.PuzzleID].IsSolvedByUserTeam;
                }

                var stats = new TeamPuzzleStats()
                {
                    PuzzleName = data.PuzzleName,
                    SolveCount = data.SolveCount,
                    SortOrder = i,
                    Fastest = fastest,
                    IsSolved = isSolved
                };

                puzzles.Add(stats);
            }

            if (this.StateFilter == PuzzleStateFilter.Unsolved)
            {
                puzzles = puzzles.Where(stats => !stats.IsSolved).ToList();
            }

            switch (this.TeamPuzzleSort ?? TeamPuzzleDefaultSort)
            {
                case TeamPuzzleSortOrder.RankAscending:
                    puzzles.Sort((rhs, lhs) => (rhs.SortOrder - lhs.SortOrder));
                    break;
                case TeamPuzzleSortOrder.RankDescending:
                    puzzles.Sort((rhs, lhs) => (lhs.SortOrder - rhs.SortOrder));
                    break;
                case TeamPuzzleSortOrder.CountAscending:
                    puzzles.Sort((rhs, lhs) => (rhs.SolveCount - lhs.SolveCount));
                    break;
                case TeamPuzzleSortOrder.CountDescending:
                    puzzles.Sort((rhs, lhs) => (lhs.SolveCount - rhs.SolveCount));
                    break;
                case TeamPuzzleSortOrder.PuzzleAscending:
                    puzzles.Sort((rhs, lhs) => (String.Compare(rhs.PuzzleName,
                                                               lhs.PuzzleName,
                                                               StringComparison.OrdinalIgnoreCase)));

                    break;
                case TeamPuzzleSortOrder.PuzzleDescending:
                    puzzles.Sort((rhs, lhs) => (String.Compare(lhs.PuzzleName,
                                                               rhs.PuzzleName,
                                                               StringComparison.OrdinalIgnoreCase)));

                    break;
                default:
                    throw new ArgumentException($"unknown sort: {this.TeamPuzzleSort}");
            }

            this.TeamPuzzles = puzzles;
            if (this.TeamPuzzles.Count == 0)
            {
                this.TeamSectionNotShowMessage = "No puzzles in this section have been solved yet.";
            }
        }

        private async Task updateSinglePlayerPuzzleStats(HashSet<int> authorPuzzleIds)
        {
            if (!Event.ShouldShowSinglePlayerPuzzles)
            {
                this.SinglePlayerPuzzles = new List<SinglePlayerPuzzleStats>();
                return;
            }

            // Get the page data: puzzle and solve count
            // Puzzle solve counts
            var solveCounts = await (from statePerPlayer in _context.SinglePlayerPuzzleStatePerPlayer
                                     where statePerPlayer.Puzzle.IsPuzzle &&
                                     statePerPlayer.UnlockedTime != null &&
                                     statePerPlayer.Puzzle.Event == Event &&
                                    (EventRole != EventRole.author || authorPuzzleIds.Contains(statePerPlayer.PuzzleID))
                                     let puzzleToGroup = new
                                     {
                                         PuzzleID = statePerPlayer.Puzzle.ID,
                                         PuzzleName = statePerPlayer.Puzzle.Name,
                                         PuzzleUserId = statePerPlayer.PlayerID,
                                         IsUnlocked = statePerPlayer.UnlockedTime.HasValue,
                                         SolveTime = statePerPlayer.SolvedTime
                                     } // Using 'let' to work around EF grouping limitations (https://www.codeproject.com/Questions/5266406/Invalidoperationexception-the-LINQ-expression-for)
                                     group puzzleToGroup by puzzleToGroup.PuzzleID into puzzleGroup
                                     select new
                                     {
                                         PuzzleID = puzzleGroup.Key,
                                         PuzzleName = puzzleGroup.First().PuzzleName,
                                         SolveCount = puzzleGroup.Where(puzzle => puzzle.SolveTime.HasValue).Count(),
                                         CurrentUserInfo = puzzleGroup.FirstOrDefault(grouping => grouping.PuzzleUserId == this.LoggedInUser.ID)
                                     })
                                     .ToListAsync();

            // Filter to unlocked puzzles
            HashSet<int> unlockedPuzzleIds = (await _context.SinglePlayerPuzzleUnlockStates
                .Where(unlockState => unlockState.Puzzle.EventID == Event.ID
                    && unlockState.UnlockedTime.HasValue
                   && (EventRole != EventRole.author || authorPuzzleIds.Contains(unlockState.PuzzleID)))
                .Select(unlockSate => unlockSate.PuzzleID)
                .ToListAsync())
                .ToHashSet();

            this.SinglePlayerPuzzles = solveCounts
                .Where(solveInfo =>
                    solveInfo.CurrentUserInfo?.IsUnlocked == true
                    || (unlockedPuzzleIds.Contains(solveInfo.PuzzleID)
                        && (solveInfo.CurrentUserInfo == null || solveInfo.CurrentUserInfo.IsUnlocked)))
                .Select(solveInfo => new SinglePlayerPuzzleStats
                {
                    PuzzleName = solveInfo.PuzzleName,
                    SolveCount = solveInfo.SolveCount,
                    IsSolved = solveInfo.CurrentUserInfo != null && solveInfo.CurrentUserInfo.SolveTime.HasValue
                })
                .ToList();

            if (this.StateFilter == PuzzleStateFilter.Unsolved)
            {
                this.SinglePlayerPuzzles = this.SinglePlayerPuzzles.Where(stats => !stats.IsSolved).ToList();
            }

            switch (this.SinglePlayerPuzzleSort ?? SinglePlayerPuzzleDefaultSort)
            {
                case SinglePlayerPuzzleSortOrder.CountAscending:
                    this.SinglePlayerPuzzles.Sort((rhs, lhs) => (rhs.SolveCount - lhs.SolveCount));
                    break;
                case SinglePlayerPuzzleSortOrder.CountDescending:
                    this.SinglePlayerPuzzles.Sort((rhs, lhs) => (lhs.SolveCount - rhs.SolveCount));
                    break;
                case SinglePlayerPuzzleSortOrder.PuzzleAscending:
                    this.SinglePlayerPuzzles.Sort((rhs, lhs) => (String.Compare(rhs.PuzzleName,
                                                               lhs.PuzzleName,
                                                               StringComparison.OrdinalIgnoreCase)));

                    break;
                case SinglePlayerPuzzleSortOrder.PuzzleDescending:
                    this.SinglePlayerPuzzles.Sort((rhs, lhs) => (String.Compare(lhs.PuzzleName,
                                                               rhs.PuzzleName,
                                                               StringComparison.OrdinalIgnoreCase)));

                    break;
                default:
                    throw new ArgumentException($"unknown sort: {this.SinglePlayerPuzzleSort}");
            }

            if (this.SinglePlayerPuzzles.Count == 0)
            {
                this.SinglePlayerPuzzleSectionNotShowMessage = "No puzzles in this section have been released yet.";
            }
        }

        private HashSet<int> GetAuthorPuzzleIds()
        {
            if (EventRole == EventRole.author)
            {
                return _context.PuzzleAuthors
                    .Where(author => author.AuthorID == LoggedInUser.ID
                        && author.Puzzle.EventID == Event.ID)
                    .Select(author => author.PuzzleID)
                    .ToHashSet();
            }
            else
            {
                return new HashSet<int>();
            }
        }

        public TeamPuzzleSortOrder? SortForColumnLink(TeamPuzzleSortOrder ascendingSort, TeamPuzzleSortOrder descendingSort)
        {
            TeamPuzzleSortOrder result = ascendingSort;

            if (result == (this.TeamPuzzleSort ?? TeamPuzzleDefaultSort))
            {
                result = descendingSort;
            }

            if (result == TeamPuzzleDefaultSort)
            {
                return null;
            }

            return result;
        }

        public SinglePlayerPuzzleSortOrder? SortForColumnLink(SinglePlayerPuzzleSortOrder ascendingSort, SinglePlayerPuzzleSortOrder descendingSort)
        {
            SinglePlayerPuzzleSortOrder result = ascendingSort;

            if (result == (this.SinglePlayerPuzzleSort ?? SinglePlayerPuzzleDefaultSort))
            {
                result = descendingSort;
            }

            if (result == SinglePlayerPuzzleDefaultSort)
            {
                return null;
            }

            return result;
        }

        public class TeamPuzzleStats
        {
            public string PuzzleName;
            public int SolveCount;
            public int SortOrder;
            public FastRecord[] Fastest;
            public bool IsSolved;
        }

        public class SinglePlayerPuzzleStats
        {
            public string PuzzleName;
            public int SolveCount;
            public bool IsSolved;
        }

        public class FastRecord
        {
            public int ID;
            public string Name;
            public TimeSpan? Time;
        }

        public enum TeamPuzzleSortOrder
        {
            RankAscending,
            RankDescending,
            PuzzleAscending,
            PuzzleDescending,
            CountAscending,
            CountDescending
        }

        public enum SinglePlayerPuzzleSortOrder
        {
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
