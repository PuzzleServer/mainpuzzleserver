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

        public List<HintView> HintViews { get; set; }

        public Puzzle Puzzle { get; set; }

        public Team  Team { get; set; }

        public SortOrder? Sort { get; set; }

        public const SortOrder DefaultSort = SortOrder.TimeDescending;

        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? teamId, SortOrder? sort)
        {
            Sort = sort;

            if (puzzleId == null)
            {
                if (EventRole == EventRole.admin)
                {
                    if (teamId == null)
                    {
                        HintViews = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.Team.Event == Event)
                            .Select(hspt => new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.Name, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime  })
                            .ToListAsync();
                    }
                    else
                    {
                        HintViews = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.TeamID == teamId)
                            .Select(hspt => new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.Name, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime })
                            .ToListAsync();
                    }
                }
                else
                {
                    if (teamId == null)
                    {
                        HintViews = await (from p in UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                                           join hspt in _context.HintStatePerTeam on p equals hspt.Hint.Puzzle
                                           where hspt.UnlockTime != null
                                           select new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.Name, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime })
                                           .ToListAsync();
                    }
                    else
                    {
                        HintViews = await (from p in UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                                           join hspt in _context.HintStatePerTeam on p equals hspt.Hint.Puzzle
                                           where hspt.UnlockTime != null && hspt.TeamID == teamId
                                           select new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.Name, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime })
                                           .ToListAsync();
                    }
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

                if (teamId == null)
                {
                    HintViews = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.Hint.Puzzle == Puzzle)
                            .Select(hspt => new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.Name, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime })
                            .ToListAsync();
                }
                else
                {
                    HintViews = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.TeamID == teamId && h.Hint.Puzzle == Puzzle)
                            .Select(hspt => new HintView { TeamId = hspt.TeamID, TeamName = hspt.Team.Name, PuzzleId = hspt.Hint.Puzzle.ID, PuzzleName = hspt.Hint.Puzzle.Name, Description = hspt.Hint.Description, Cost = hspt.Hint.Cost, UnlockTime = hspt.UnlockTime })
                            .ToListAsync();
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
                case SortOrder.TeamAscending:
                    HintViews.Sort((a, b) => a.TeamName.CompareTo(b.TeamName));
                    break;
                case SortOrder.TeamDescending:
                    HintViews.Sort((a, b) => -a.TeamName.CompareTo(b.TeamName));
                    break;
                case SortOrder.PuzzleAscending:
                    HintViews.Sort((a, b) => a.PuzzleName.CompareTo(b.PuzzleName));
                    break;
                case SortOrder.PuzzleDescending:
                    HintViews.Sort((a, b) => -a.PuzzleName.CompareTo(b.PuzzleName));
                    break;
                case SortOrder.DescriptionAscending:
                    HintViews.Sort((a, b) => a.Description.CompareTo(b.Description));
                    break;
                case SortOrder.DescriptionDescending:
                    HintViews.Sort((a, b) => -a.Description.CompareTo(b.Description));
                    break;
                case SortOrder.CostAscending:
                    HintViews.Sort((a, b) => a.Cost.CompareTo(b.Cost));
                    break;
                case SortOrder.CostDescending:
                    HintViews.Sort((a, b) => -a.Cost.CompareTo(b.Cost));
                    break;
                case SortOrder.TimeAscending:
                    HintViews.Sort((a, b) => a.UnlockTime.Value.CompareTo(b.UnlockTime.Value));
                    break;
                case SortOrder.TimeDescending:
                    HintViews.Sort((a, b) => -a.UnlockTime.Value.CompareTo(b.UnlockTime.Value));
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

        public class HintView
        {
            public int TeamId { get; set; }
            public string TeamName { get; set; }
            public int PuzzleId { get; set; }
            public string PuzzleName { get; set; }
            public string Description { get; set; }
            public int Cost { get; set; }
            public DateTime? UnlockTime { get; set; }
        }

        public enum SortOrder
        {
            TeamAscending,
            TeamDescending,
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