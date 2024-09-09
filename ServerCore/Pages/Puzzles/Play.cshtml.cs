using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    /// <summary>
    /// Model for the player's "Puzzles" page. Shows a list of the all the player puzzles (both team puzzles and single player puzzles).
    /// </summary>
    [Authorize()]
    public class PlayModel : EventSpecificPageModel
    {
        // see https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-2.1 to make this sortable!

        public PlayModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IEnumerable<PuzzleView> VisibleTeamPuzzleViews { get; set; }

        public IEnumerable<PuzzleView> VisibleSinglePlayerPuzzleViews { get; set; }
        public IEnumerable<AlphaPuzzleView> UnclaimedAlphaPuzzleViews { get; set; }

        public PuzzleStateFilter? StateFilter { get; set; }

        public SortOrder? TeamPuzzleSort { get; set; }

        public SortOrder? SinglePlayerPuzzleSort { get; set; }

        private const SortOrder DefaultSort = SortOrder.GroupAscending;

        public bool ShowAnswers { get; set; }

        public bool AllowFeedback { get; set; }

        public bool AnyErrata { get; set; }

        public Team Team { get; set; }

        public bool HasLiveEvents { get; set; }

        public async Task OnGetAsync(
            SortOrder? teamPuzzleSort,
            SortOrder? singlePlayerPuzzleSort,
            PuzzleStateFilter? stateFilter)
        {
            TeamPuzzleSort = teamPuzzleSort;
            SinglePlayerPuzzleSort = singlePlayerPuzzleSort;
            StateFilter = stateFilter;
            ShowAnswers = Event.AnswersAvailableBegin <= DateTime.UtcNow;
            AllowFeedback = Event.AllowFeedback;

            var puzzleFiles = new Dictionary<int, ContentFile>();
            var answerFiles = new Dictionary<int, ContentFile>();
            await (from file in _context.ContentFiles
                where file.Event == Event && (file.FileType == ContentFileType.Puzzle || file.FileType == ContentFileType.Answer)
                select file)
                .ForEachAsync(file => 
                {
                    if (file.FileType == ContentFileType.Puzzle)
                    {
                        puzzleFiles.Add(file.PuzzleID, file);
                    }
                    else if (file.FileType == ContentFileType.Answer)
                    {
                        answerFiles.Add(file.PuzzleID, file);
                    }
                });

            VisibleSinglePlayerPuzzleViews = this.GetVisibleSinglePlayerPuzzleViews(singlePlayerPuzzleSort).ToList();
            this.PopulatePuzzleViewsWithFiles(VisibleSinglePlayerPuzzleViews, puzzleFiles, answerFiles);
            AnyErrata = VisibleSinglePlayerPuzzleViews.Any(v => v.Errata != null);

            if (!Event.EphemeralHackKillLiveEventPage)
            {
                HasLiveEvents = (await LiveEventHelper.GetLiveEventsForEvent(_context, Event, false, false)).Any();
            }

            Team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            if (Team != null)
            {
                await PuzzleStateHelper.CheckForTimedUnlocksAsync(_context, Event, Team);
                VisibleTeamPuzzleViews = (await this.GetVisibleTeamPlayerPuzzleViews(teamPuzzleSort, Team)).ToList();
                this.PopulatePuzzleViewsWithFiles(VisibleTeamPuzzleViews, puzzleFiles, answerFiles);
                if (!AnyErrata)
                {
                    AnyErrata = VisibleTeamPuzzleViews.Any(v => v.Errata != null);
                }
            }

            if (Event.IsAlphaTestingEvent)
            {
                HashSet<int> visibleIDs = new HashSet<int>();
                List<AlphaPuzzleView> notYetVisibleUnclaimedViews = new List<AlphaPuzzleView>();

                foreach (var view in VisibleTeamPuzzleViews) { visibleIDs.Add(view.ID); }
                foreach (var view in VisibleSinglePlayerPuzzleViews) { visibleIDs.Add(view.ID); }

                var unclaimedViews = await _context.Puzzles
                    .Where(p => p.EventID == Event.ID && p.AlphaTestsNeeded > 0)
                    .Select(p => new AlphaPuzzleView { ID = p.ID, Name = p.Name, AlphaTestsNeeded = p.AlphaTestsNeeded })
                    .ToListAsync();

                foreach (var alphaView in unclaimedViews)
                {
                    if (!visibleIDs.Contains(alphaView.ID))
                    {
                        notYetVisibleUnclaimedViews.Add(alphaView);
                    }
                }

                UnclaimedAlphaPuzzleViews = notYetVisibleUnclaimedViews;
            }
        }

        public SortOrder? SortForColumnLink(SortOrder? currentSort, SortOrder ascendingSort, SortOrder descendingSort)
        {
            if (currentSort == null)
            {
                return ascendingSort;
            }
            else if (currentSort == ascendingSort)
            {
                return descendingSort;
            }
            else
            {
                return ascendingSort;
            }
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

        public class AlphaPuzzleView
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int AlphaTestsNeeded { get; set; }
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

        private async Task<IEnumerable<PuzzleView>> GetVisibleTeamPlayerPuzzleViews(SortOrder? sortOrder, Team team)
        {
            // If the event has not yet begun, no puzzles yet
            if (DateTime.UtcNow < Event.EventBegin && !team.IsDisqualified)
            {
                return Enumerable.Empty<PuzzleView>();
            }

            // all puzzles for this event that are real puzzles
            var puzzlesInEventQ = _context.Puzzles.Where(puzzle => puzzle.Event.ID == this.Event.ID && puzzle.IsPuzzle && !puzzle.IsForSinglePlayer);

            // unless we're in a global lockout, then filter to those!
            var puzzlesCausingGlobalLockoutQ = PuzzleStateHelper.PuzzlesCausingGlobalLockout(_context, Event, team);
            if (await puzzlesCausingGlobalLockoutQ.AnyAsync())
            {
                puzzlesInEventQ = puzzlesCausingGlobalLockoutQ;
            }

            // all puzzle states for this team that are unlocked (note: IsUnlocked bool is going to harm perf, just null check the time here)
            // Note that it's OK if some puzzles do not yet have a state record; those puzzles are clearly still locked and hence invisible.
            // All puzzles will show if all answers have been released)
            var stateForTeamQ = _context.PuzzleStatePerTeam.Where(state => state.TeamID == this.Team.ID && (ShowAnswers || state.UnlockedTime != null));

            // join 'em (note: just getting all properties for max flexibility, can pick and choose columns for perf later)
            // Note: EF gotcha is that you have to join into anonymous types in order to not lose valuable stuff
            var visiblePuzzlesQ = from Puzzle puzzle in puzzlesInEventQ
                                  join PuzzleStatePerTeam pspt in stateForTeamQ on puzzle.ID equals pspt.PuzzleID
                                  select new PuzzleView { ID = puzzle.ID, Group = puzzle.Group, OrderInGroup = puzzle.OrderInGroup, Name = puzzle.Name, CustomUrl = puzzle.CustomURL, CustomSolutionUrl = puzzle.CustomSolutionURL, Errata = puzzle.Errata, SolvedTime = pspt.SolvedTime, PieceMetaUsage = puzzle.PieceMetaUsage };

            if (this.StateFilter == PuzzleStateFilter.Unsolved)
            {
                visiblePuzzlesQ = visiblePuzzlesQ.Where(puzzles => puzzles.SolvedTime == null);
            }

            return this.GetSortedView(visiblePuzzlesQ, sortOrder);
        }

        private IEnumerable<PuzzleView> GetVisibleSinglePlayerPuzzleViews(SortOrder? sortOrder)
        {
            Dictionary<int, PuzzleView> singlePlayerPuzzleViewDict = _context.SinglePlayerPuzzleUnlockStates
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
            var singlePlayerPuzzleStatePerPlayer = _context.SinglePlayerPuzzleStatePerPlayer
                .Where(state => state.PlayerID == LoggedInUser.ID && state.Puzzle.EventID == Event.ID);
            foreach (SinglePlayerPuzzleStatePerPlayer statePerPlayer in singlePlayerPuzzleStatePerPlayer)
            {
                singlePlayerPuzzleViewDict[statePerPlayer.PuzzleID].SolvedTime = statePerPlayer.SolvedTime;
                singlePlayerPuzzleViewDict[statePerPlayer.PuzzleID].UnlockedTime = statePerPlayer.UnlockedTime;
            }

            IEnumerable<PuzzleView> visibleSinglePlayerPuzzlesQ = singlePlayerPuzzleViewDict.Values.AsEnumerable().Where(puzzleView => ShowAnswers || puzzleView.UnlockedTime != null);
            visibleSinglePlayerPuzzlesQ = this.GetSortedView(visibleSinglePlayerPuzzlesQ, sortOrder);
            if (StateFilter == PuzzleStateFilter.Unsolved)
            {
                visibleSinglePlayerPuzzlesQ = visibleSinglePlayerPuzzlesQ.Where(puzzles => puzzles.SolvedTime == null);
            }

            return visibleSinglePlayerPuzzlesQ;
        }

        private IEnumerable<PuzzleView> GetSortedView(IEnumerable<PuzzleView> puzzleViews, SortOrder? sortOrder)
        {
            SortOrder actualSortOrder = sortOrder.HasValue ? sortOrder.Value : DefaultSort;

            switch (actualSortOrder)
            {
                case SortOrder.PuzzleAscending:
                    return puzzleViews.OrderBy(pv => pv.Name);
                case SortOrder.PuzzleDescending:
                    return puzzleViews.OrderByDescending(pv => pv.Name);
                case SortOrder.GroupAscending:
                    return puzzleViews.OrderBy(pv => pv.Group).ThenBy(pv => pv.OrderInGroup).ThenBy(pv => pv.Name);
                case SortOrder.GroupDescending:
                    return puzzleViews.OrderByDescending(pv => pv.Group).ThenByDescending(pv => pv.OrderInGroup).ThenByDescending(pv => pv.Name);
                case SortOrder.SolveAscending:
                    return puzzleViews.OrderBy(pv => pv.SolvedTime ?? DateTime.MaxValue);
                case SortOrder.SolveDescending:
                    return puzzleViews.OrderByDescending(pv => pv.SolvedTime ?? DateTime.MaxValue);
                default:
                    throw new ArgumentException($"unknown sort: {sortOrder}");
            }
        }

        private void PopulatePuzzleViewsWithFiles(
            IEnumerable<PuzzleView> puzzleViews,
            Dictionary<int, ContentFile> puzzleFiles,
            Dictionary<int, ContentFile> answerFiles)
        {
            foreach (var puzzleView in puzzleViews)
            {
                puzzleFiles.TryGetValue(puzzleView.ID, out ContentFile content);
                puzzleView.Content = content;

                if (ShowAnswers)
                {
                    answerFiles.TryGetValue(puzzleView.ID, out ContentFile answer);
                    puzzleView.Answer = answer;
                }
            }
        }
    }
}