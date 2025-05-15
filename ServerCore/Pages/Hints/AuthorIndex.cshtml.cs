using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class AuthorIndexModel : EventSpecificPageModel
    {
        public AuthorIndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public bool ShouldShowTeamHints { get; set; }

        public List<HintView> HintViews { get; set; }

        public bool ShouldShowSinglePlayerPuzzleHints { get; set; }

        public List<SinglePlayerPuzzleHintView> SinglePlayerPuzzleHintViews { get; set; }

        public Puzzle Puzzle { get; set; }

        public Team  Team { get; set; }

        public SortOrder? Sort { get; set; }

        public const SortOrder DefaultSort = SortOrder.TimeDescending;

        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? teamId, SortOrder? sort)
        {
            Sort = sort;

            if (puzzleId == null)
            {
                ShouldShowTeamHints = true;
                ShouldShowSinglePlayerPuzzleHints = true;
                if (EventRole == EventRole.admin)
                {
                    if (teamId == null)
                    {
                        HintViews = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.Team.Event == Event)
                            .Select(hspt => new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.PlaintextName, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime  })
                            .ToListAsync();
                    }
                    else
                    {
                        HintViews = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.TeamID == teamId)
                            .Select(hspt => new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.PlaintextName, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime })
                            .ToListAsync();
                    }

                    SinglePlayerPuzzleHintViews = await _context.SinglePlayerPuzzleHintStatePerPlayer.Where((h) => h.UnlockTime != null && h.Hint.Puzzle.Event == Event)
                            .Select(hspp => new SinglePlayerPuzzleHintView { PlayerId = hspp.PlayerID, PlayerName = hspp.Player.Name, PuzzleId = hspp.Hint.Puzzle.ID, PuzzleName = hspp.Hint.Puzzle.PlaintextName, Description = hspp.Hint.Description, Cost = hspp.Hint.Cost, UnlockTime = hspp.UnlockTime })
                            .ToListAsync();
                }
                else
                {
                    if (teamId == null)
                    {
                        HintViews = await (from p in UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                                           join hspt in _context.HintStatePerTeam on p equals hspt.Hint.Puzzle
                                           where hspt.UnlockTime != null
                                           select new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.PlaintextName, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime })
                                           .ToListAsync();
                    }
                    else
                    {
                        HintViews = await (from p in UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                                           join hspt in _context.HintStatePerTeam on p equals hspt.Hint.Puzzle
                                           where hspt.UnlockTime != null && hspt.TeamID == teamId
                                           select new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.PlaintextName, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime })
                                           .ToListAsync();
                    }

                    SinglePlayerPuzzleHintViews = await (from p in UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                                                         join hspp in _context.SinglePlayerPuzzleHintStatePerPlayer on p.ID equals hspp.Hint.PuzzleID
                                                         where hspp.UnlockTime != null
                                                         select new SinglePlayerPuzzleHintView { PlayerId = hspp.PlayerID, PlayerName = hspp.Player.Name, PuzzleId = hspp.Hint.Puzzle.ID, PuzzleName = hspp.Hint.Puzzle.PlaintextName, Description = hspp.Hint.Description, Cost = hspp.Hint.Cost, UnlockTime = hspp.UnlockTime })
                                                         .ToListAsync();
                }
            }
            else
            {
                Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();

                if (Puzzle == null)
                {
                    return NotFound();
                }

                if (EventRole == EventRole.author && !await UserEventHelper.IsAuthorOfPuzzle(_context, Puzzle, LoggedInUser))
                {
                    return Forbid();
                }


                ShouldShowSinglePlayerPuzzleHints = Puzzle.IsForSinglePlayer;
                ShouldShowTeamHints = !ShouldShowSinglePlayerPuzzleHints;

                if (Puzzle.IsForSinglePlayer)
                {
                    HintViews = new List<HintView>();
                    SinglePlayerPuzzleHintViews = await _context.SinglePlayerPuzzleHintStatePerPlayer.Where((h) => h.UnlockTime != null && h.Hint.PuzzleID == puzzleId)
                            .Select(hspp => new SinglePlayerPuzzleHintView { PlayerId = hspp.PlayerID, PlayerName = hspp.Player.Name, PuzzleId = hspp.Hint.Puzzle.ID, PuzzleName = hspp.Hint.Puzzle.PlaintextName, Description = hspp.Hint.Description, Cost = hspp.Hint.Cost, UnlockTime = hspp.UnlockTime })
                            .ToListAsync();
                }
                else
                {
                    SinglePlayerPuzzleHintViews = new List<SinglePlayerPuzzleHintView>();
                    if (teamId == null)
                    {
                        HintViews = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.Hint.Puzzle == Puzzle)
                                .Select(hspt => new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.PlaintextName, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime })
                                .ToListAsync();
                    }
                    else
                    {
                        HintViews = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.TeamID == teamId && h.Hint.Puzzle == Puzzle)
                                .Select(hspt => new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.PlaintextName, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime })
                                .ToListAsync();
                    }
                }
            }

            if (teamId != null)
            {
                Team = await _context.Teams.Where(m => m.ID == teamId).FirstOrDefaultAsync();

                if (Team == null)
                {
                    return NotFound();
                }
            }

            switch (sort ?? DefaultSort)
            {
                case SortOrder.NameAscending:
                    HintViews.Sort((a, b) => a.TeamName.CompareTo(b.TeamName));
                    SinglePlayerPuzzleHintViews.Sort((a, b) => a.PlayerName.CompareTo(b.PlayerName));
                    break;
                case SortOrder.NameDescending:
                    HintViews.Sort((a, b) => -a.TeamName.CompareTo(b.TeamName));
                    SinglePlayerPuzzleHintViews.Sort((a, b) => -a.PlayerName.CompareTo(b.PlayerName));
                    break;
                case SortOrder.PuzzleAscending:
                    HintViews.Sort((a, b) => a.PuzzleName.CompareTo(b.PuzzleName));
                    SinglePlayerPuzzleHintViews.Sort((a, b) => a.PuzzleName.CompareTo(b.PuzzleName));
                    break;
                case SortOrder.PuzzleDescending:
                    HintViews.Sort((a, b) => -a.PuzzleName.CompareTo(b.PuzzleName));
                    SinglePlayerPuzzleHintViews.Sort((a, b) => -a.PuzzleName.CompareTo(b.PuzzleName));
                    break;
                case SortOrder.DescriptionAscending:
                    HintViews.Sort((a, b) => a.Description.CompareTo(b.Description));
                    SinglePlayerPuzzleHintViews.Sort((a, b) => a.Description.CompareTo(b.Description));
                    break;
                case SortOrder.DescriptionDescending:
                    HintViews.Sort((a, b) => -a.Description.CompareTo(b.Description));
                    SinglePlayerPuzzleHintViews.Sort((a, b) => -a.Description.CompareTo(b.Description));
                    break;
                case SortOrder.CostAscending:
                    HintViews.Sort((a, b) => a.Cost.CompareTo(b.Cost));
                    SinglePlayerPuzzleHintViews.Sort((a, b) => a.Cost.CompareTo(b.Cost));
                    break;
                case SortOrder.CostDescending:
                    HintViews.Sort((a, b) => -a.Cost.CompareTo(b.Cost));
                    SinglePlayerPuzzleHintViews.Sort((a, b) => -a.Cost.CompareTo(b.Cost));
                    break;
                case SortOrder.TimeAscending:
                    HintViews.Sort((a, b) => a.UnlockTime.Value.CompareTo(b.UnlockTime.Value));
                    SinglePlayerPuzzleHintViews.Sort((a, b) => a.UnlockTime.Value.CompareTo(b.UnlockTime.Value));
                    break;
                case SortOrder.TimeDescending:
                    HintViews.Sort((a, b) => -a.UnlockTime.Value.CompareTo(b.UnlockTime.Value));
                    SinglePlayerPuzzleHintViews.Sort((a, b) => -a.UnlockTime.Value.CompareTo(b.UnlockTime.Value));
                    break;
            }

            return Page();
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

        public class HintView : HintViewBase
        {
            public int TeamId { get; set; }
            public string TeamName { get; set; }
        }

        public class SinglePlayerPuzzleHintView : HintViewBase
        {
            public int PlayerId { get; set; }
            public string PlayerName { get; set; }
        }

        public class HintViewBase
        {
            public int PuzzleId { get; set; }
            public string PuzzleName { get; set; }
            public string Description { get; set; }
            public int Cost { get; set; }
            public DateTime? UnlockTime { get; set; }
        }

        public enum SortOrder
        {
            NameAscending,
            NameDescending,
            PuzzleAscending,
            PuzzleDescending,
            DescriptionAscending,
            DescriptionDescending,
            CostAscending,
            CostDescending,
            TimeAscending,
            TimeDescending
        }
    }
}