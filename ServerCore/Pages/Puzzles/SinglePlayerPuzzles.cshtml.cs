using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    /// <summary>
    /// Model for the player's "Puzzles" page. Shows a list of the player's individual unsolved puzzles, with sorting options.
    /// </summary>
    public class SinglePlayerPuzzlesModel : EventSpecificPageModel
    {
        // see https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-2.1 to make this sortable!

        public SinglePlayerPuzzlesModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<PuzzleView> PuzzleViews { get; set; }

        public PuzzleStateFilter? StateFilter { get; set; }

        public SortOrder? Sort { get; set; }

        private const SortOrder DefaultSort = SortOrder.GroupAscending;

        public bool ShowAnswers { get; set; }

        public bool AllowFeedback { get; set; }

        public async Task OnGetAsync(SortOrder? sort, PuzzleStateFilter? stateFilter)
        {
            Sort = sort;
            StateFilter = stateFilter;

            ShowAnswers = Event.AnswersAvailableBegin <= DateTime.UtcNow;
            AllowFeedback = Event.AllowFeedback;

            Dictionary<int, PuzzleView> puzzleViewDict = _context.SinglePlayerPuzzleUnlockStates
                .Where(unlockState => unlockState.Puzzle.EventID == Event.ID)
                .ToDictionary(unlockState => unlockState.PuzzleID, unlockState => new PuzzleView
                {
                    ID = unlockState.Puzzle.ID,
                    Group = unlockState.Puzzle.Group,
                    OrderInGroup = unlockState.Puzzle.OrderInGroup,
                    Name = unlockState.Puzzle.Name,
                    CustomUrl = unlockState.Puzzle.CustomURL,
                    CustomSolutionUrl = unlockState.Puzzle.CustomSolutionURL,
                    Errata = unlockState.Puzzle.Errata,
                    UnlockedTime = unlockState.UnlockedTime,
                    SolvedTime = null,
                    PieceMetaUsage = unlockState.Puzzle.PieceMetaUsage
                });

            // Populate solve time based on statePerPlayer
            var puzzleStatePerPlayer = _context.SinglePlayerPuzzleStatePerPlayer
                .Where(state => state.PlayerID == LoggedInUser.ID && state.Puzzle.EventID == Event.ID);
            foreach (SinglePlayerPuzzleStatePerPlayer statePerPlayer in puzzleStatePerPlayer)
            {
                puzzleViewDict[statePerPlayer.PuzzleID].SolvedTime = statePerPlayer.SolvedTime;
                puzzleViewDict[statePerPlayer.PuzzleID].UnlockedTime = statePerPlayer.UnlockedTime;
            }

            var visiblePuzzlesQ = puzzleViewDict.Values.AsEnumerable().Where(puzzleView => ShowAnswers || puzzleView.UnlockedTime != null);

            switch (sort ?? DefaultSort)
            {
                case SortOrder.PuzzleAscending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(pv => pv.Name);
                    break;
                case SortOrder.PuzzleDescending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(pv => pv.Name);
                    break;
                case SortOrder.GroupAscending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(pv => pv.Group).ThenBy(pv => pv.OrderInGroup).ThenBy(pv => pv.Name);
                    break;
                case SortOrder.GroupDescending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(pv => pv.Group).ThenByDescending(pv => pv.OrderInGroup).ThenByDescending(pv => pv.Name);
                    break;
                case SortOrder.SolveAscending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(pv => pv.SolvedTime ?? DateTime.MaxValue);
                    break;
                case SortOrder.SolveDescending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(pv => pv.SolvedTime ?? DateTime.MaxValue);
                    break;
                default:
                    throw new ArgumentException($"unknown sort: {sort}");
            }

            if (StateFilter == PuzzleStateFilter.Unsolved)
            {
                visiblePuzzlesQ = visiblePuzzlesQ.Where(puzzles => puzzles.SolvedTime == null);
            }

            PuzzleViews = visiblePuzzlesQ.ToList();

            Dictionary<int, ContentFile> files = await (from file in _context.ContentFiles
                                                        where file.Event == Event && file.FileType == ContentFileType.Puzzle
                                                        select file).ToDictionaryAsync(file => file.PuzzleID);

            foreach (var puzzleView in PuzzleViews)
            {
                files.TryGetValue(puzzleView.ID, out ContentFile content);
                puzzleView.Content = content;
            }

            if (ShowAnswers)
            {
                Dictionary<int, ContentFile> answers = await (from file in _context.ContentFiles
                                                              where file.Event == Event && file.FileType == ContentFileType.Answer
                                                              select file).ToDictionaryAsync(file => file.PuzzleID);

                foreach (var puzzleView in PuzzleViews)
                {
                    answers.TryGetValue(puzzleView.ID, out ContentFile answer);
                    puzzleView.Answer = answer;
                }
            }
        }

        public SortOrder? SortForColumnLink(SortOrder ascendingSort, SortOrder descendingSort)
        {
            SortOrder result = ascendingSort;

            if (result == (Sort ?? DefaultSort))
            {
                result = descendingSort;
            }

            if (result == DefaultSort)
            {
                return null;
            }

            return result;
        }

        public class PuzzleView
        {
            public int ID { get; set; }
            public string Group { get; set; }
            public int OrderInGroup { get; set; }
            public string Name { get; set; }
            public string Errata { get; set; }
            public string CustomUrl { get; set; }
            public string CustomSolutionUrl { get; set; }
            public DateTime? UnlockedTime { get; set; }
            public DateTime? SolvedTime { get; set; }
            public ContentFile Content { get; set; }
            public ContentFile Answer { get; set; }
            public PieceMetaUsage PieceMetaUsage { get; set; }
        }

        public enum SortOrder
        {
            PuzzleAscending,
            PuzzleDescending,
            GroupAscending,
            GroupDescending,
            SolveAscending,
            SolveDescending
        }

        public enum PuzzleStateFilter
        {
            All,
            Unsolved
        }
    }
}