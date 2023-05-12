using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    /// <summary>
    /// Model for the player's "Puzzles" page. Shows a list of the player's individual unsolved puzzles, with sorting options.
    /// </summary>
    [Authorize(Policy = "IsPlayer")]
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

            // all puzzles for this event that are real puzzles
            var puzzlesInEventQ = _context.Puzzles.Where(puzzle => puzzle.Event.ID == Event.ID && puzzle.IsPuzzle && puzzle.IsForSinglePlayer);

            // all puzzle states that are unlocked (note: IsUnlocked bool is going to harm perf, just null check the time here)
            // Note that it's OK if some puzzles do not yet have a state record; those puzzles are clearly still locked and hence invisible.
            // All puzzles will show if all answers have been released)
            var puzzleStateQ = _context.SinglePlayerPuzzleStatePerPlayer.Where(state => state.UserID == LoggedInUser.ID && (ShowAnswers || state.UnlockedTime != null));

            // join 'em (note: just getting all properties for max flexibility, can pick and choose columns for perf later)
            // Note: EF gotcha is that you have to join into anonymous types in order to not lose valuable stuff
            var visiblePuzzlesQ = from Puzzle puzzle in puzzlesInEventQ
                                  join SinglePlayerPuzzleStatePerPlayer pspi in puzzleStateQ on puzzle.ID equals pspi.PuzzleID
                                  select new PuzzleView
                                  {
                                      ID = puzzle.ID,
                                      Group = puzzle.Group,
                                      OrderInGroup = puzzle.OrderInGroup,
                                      Name = puzzle.Name,
                                      CustomUrl = puzzle.CustomURL,
                                      CustomSolutionUrl = puzzle.CustomSolutionURL,
                                      Errata = puzzle.Errata,
                                      SolvedTime = pspi.SolvedTime,
                                      PieceMetaUsage = puzzle.PieceMetaUsage
                                  };

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

            PuzzleViews = await visiblePuzzlesQ.ToListAsync();

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
