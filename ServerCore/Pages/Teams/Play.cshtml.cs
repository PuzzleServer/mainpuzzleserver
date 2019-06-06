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

namespace ServerCore.Pages.Teams
{
    /// <summary>
    /// Model for the player's "Puzzles" page. Shows a list of the team's unsolved puzzles, with sorting options.
    /// </summary>
    [Authorize(Policy = "PlayerIsOnTeam")]
    public class PlayModel : EventSpecificPageModel
    {
        // see https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-2.1 to make this sortable!

        public PlayModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<PuzzleView> PuzzleViews { get; set; }

        public PuzzleStateFilter? StateFilter { get; set; }

        public int TeamID { get; set; }

        public SortOrder? Sort { get; set; }

        private const SortOrder DefaultSort = SortOrder.GroupAscending;

        public bool ShowAnswers { get; set; }

        public bool AllowFeedback { get; set; }

        public async Task OnGetAsync(SortOrder? sort, int teamId, PuzzleStateFilter? stateFilter)
        {
            TeamID = teamId;
            Team myTeam = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            if (myTeam != null)
            {
                this.TeamID = myTeam.ID;
                await PuzzleStateHelper.CheckForTimedUnlocksAsync(_context, Event, myTeam);
            }
            else
            {
                throw new Exception("Not currently registered for a team");
            }
            this.Sort = sort;
            this.StateFilter = stateFilter;

            ShowAnswers = Event.AnswersAvailableBegin <= DateTime.UtcNow;
            AllowFeedback = Event.AllowFeedback;

            // all puzzles for this event that are real puzzles
            var puzzlesInEventQ = _context.Puzzles.Where(puzzle => puzzle.Event.ID == this.Event.ID && puzzle.IsPuzzle);

            // unless we're in a global lockout, then filter to those!
            var puzzlesCausingGlobalLockoutQ = PuzzleStateHelper.PuzzlesCausingGlobalLockout(_context, Event, myTeam);
            if (await puzzlesCausingGlobalLockoutQ.AnyAsync())
            {
                puzzlesInEventQ = puzzlesCausingGlobalLockoutQ;
            }

            // all puzzle states for this team that are unlocked (note: IsUnlocked bool is going to harm perf, just null check the time here)
            // Note that it's OK if some puzzles do not yet have a state record; those puzzles are clearly still locked and hence invisible.
            var stateForTeamQ = _context.PuzzleStatePerTeam.Where(state => state.TeamID == this.TeamID && state.UnlockedTime != null);

            // join 'em (note: just getting all properties for max flexibility, can pick and choose columns for perf later)
            // Note: EF gotcha is that you have to join into anonymous types in order to not lose valuable stuff
            var visiblePuzzlesQ = from Puzzle puzzle in puzzlesInEventQ
                                  join PuzzleStatePerTeam pspt in stateForTeamQ on puzzle.ID equals pspt.PuzzleID
                                  select new PuzzleView { ID = puzzle.ID, Group = puzzle.Group, OrderInGroup = puzzle.OrderInGroup, Name = puzzle.Name, CustomUrl = puzzle.CustomURL, Errata = puzzle.Errata, SolvedTime = pspt.SolvedTime, PieceMetaUsage = puzzle.PieceMetaUsage };

            switch (sort ?? DefaultSort)
            {
                case SortOrder.PuzzleAscending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(pv => pv.Name);
                    break;
                case SortOrder.PuzzleDescending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(pv => pv.Name);
                    break;
                case SortOrder.GroupAscending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderBy(pv => pv.Group).ThenBy(pv => pv.OrderInGroup);
                    break;
                case SortOrder.GroupDescending:
                    visiblePuzzlesQ = visiblePuzzlesQ.OrderByDescending(pv => pv.Group).ThenByDescending(pv => pv.OrderInGroup);
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

            if (this.StateFilter == PuzzleStateFilter.Unsolved)
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

        public class PuzzleView
        {
            public int ID { get; set; }
            public string Group { get; set; }
            public int OrderInGroup { get; set; }
            public string Name { get; set; }
            public string Errata { get; set; }
            public string CustomUrl { get; set; }
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
