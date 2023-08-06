using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class SinglePlayerPuzzleFastestSolvesModel : EventSpecificPageModel
    {
        public List<PuzzleStats> Puzzles { get; private set; }

        public PuzzleStateFilter? StateFilter { get; set; }

        public SortOrder? Sort { get; set; }

        private const SortOrder DefaultSort = SortOrder.CountDescending;

        public SinglePlayerPuzzleFastestSolvesModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(SortOrder? sort, PuzzleStateFilter? stateFilter)
        {
            if (!Event.ShouldShowSinglePlayerPuzzles)
            {
                return RedirectToPage("./Standings");
            }

            this.Sort = sort;
            this.StateFilter = stateFilter;

            // Get the page data: puzzle and solve count
            // Puzzle solve counts
            var solveCounts = await (from statePerPlayer in _context.SinglePlayerPuzzleStatePerPlayer
                                     where statePerPlayer.Puzzle.IsPuzzle &&
                                     statePerPlayer.UnlockedTime != null &&
                                     statePerPlayer.Puzzle.Event == Event
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
                .Where(unlockState => unlockState.Puzzle.EventID == Event.ID)
                .Where(unlockState => unlockState.UnlockedTime.HasValue)
                .Select(unlockSate => unlockSate.PuzzleID)
                .ToListAsync())
                .ToHashSet();

            this.Puzzles = solveCounts
                .Where(solveInfo =>
                    solveInfo.CurrentUserInfo?.IsUnlocked == true
                    || (unlockedPuzzleIds.Contains(solveInfo.PuzzleID)
                        && (solveInfo.CurrentUserInfo == null || solveInfo.CurrentUserInfo.IsUnlocked)))
                .Select(solveInfo => new PuzzleStats
                {
                    PuzzleName = solveInfo.PuzzleName,
                    SolveCount = solveInfo.SolveCount,
                    IsSolved = solveInfo.CurrentUserInfo != null && solveInfo.CurrentUserInfo.SolveTime.HasValue
                })
                .ToList();

            if (this.StateFilter == PuzzleStateFilter.Unsolved)
            {
                this.Puzzles = this.Puzzles.Where(stats => !stats.IsSolved).ToList();
            }

            switch (sort ?? DefaultSort)
            {
                case SortOrder.CountAscending:
                    this.Puzzles.Sort((rhs, lhs) => (rhs.SolveCount - lhs.SolveCount));
                    break;
                case SortOrder.CountDescending:
                    this.Puzzles.Sort((rhs, lhs) => (lhs.SolveCount - rhs.SolveCount));
                    break;
                case SortOrder.PuzzleAscending:
                    this.Puzzles.Sort((rhs, lhs) => (String.Compare(rhs.PuzzleName,
                                                               lhs.PuzzleName,
                                                               StringComparison.OrdinalIgnoreCase)));

                    break;
                case SortOrder.PuzzleDescending:
                    this.Puzzles.Sort((rhs, lhs) => (String.Compare(lhs.PuzzleName,
                                                               rhs.PuzzleName,
                                                               StringComparison.OrdinalIgnoreCase)));

                    break;
                default:
                    throw new ArgumentException($"unknown sort: {sort}");
            }

            return Page();
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
