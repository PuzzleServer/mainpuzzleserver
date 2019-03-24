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

        public List<HintStatePerTeam> HintStatePerTeam { get; set; }

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
                        HintStatePerTeam = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.Team.Event == Event).ToListAsync();
                    }
                    else
                    {
                        HintStatePerTeam = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.TeamID == teamId).ToListAsync();
                    }
                }
                else
                {
                    // Surely there is a way to get a join to do a bunch of this work, but joins are simply not for me. Someone else can fix later.
                    var hintStatePerTeam = new List<HintStatePerTeam>();
                    var authorPuzzles = await UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser).ToListAsync();

                    if (teamId == null)
                    {
                        HintStatePerTeam = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.Team.Event == Event).ToListAsync();
                        HintStatePerTeam = HintStatePerTeam.Where((h) => authorPuzzles.Contains(h.Hint.Puzzle)).ToList();
                    }
                    else
                    {
                        HintStatePerTeam = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.TeamID == teamId).ToListAsync();
                        HintStatePerTeam = HintStatePerTeam.Where((h) => authorPuzzles.Contains(h.Hint.Puzzle)).ToList();
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
                    HintStatePerTeam = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.Hint.Puzzle == Puzzle).ToListAsync();
                }
                else
                {
                    HintStatePerTeam = await _context.HintStatePerTeam.Where((h) => h.UnlockTime != null && h.TeamID == teamId && h.Hint.Puzzle == Puzzle).ToListAsync();
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
                    HintStatePerTeam.Sort((a, b) => a.Team.Name.CompareTo(b.Team.Name));
                    break;
                case SortOrder.TeamDescending:
                    HintStatePerTeam.Sort((a, b) => -a.Team.Name.CompareTo(b.Team.Name));
                    break;
                case SortOrder.PuzzleAscending:
                    HintStatePerTeam.Sort((a, b) => a.Hint.Puzzle.Name.CompareTo(b.Hint.Puzzle.Name));
                    break;
                case SortOrder.PuzzleDescending:
                    HintStatePerTeam.Sort((a, b) => -a.Hint.Puzzle.Name.CompareTo(b.Hint.Puzzle.Name));
                    break;
                case SortOrder.DescriptionAscending:
                    HintStatePerTeam.Sort((a, b) => a.Hint.Description.CompareTo(b.Hint.Description));
                    break;
                case SortOrder.DescriptionDescending:
                    HintStatePerTeam.Sort((a, b) => -a.Hint.Description.CompareTo(b.Hint.Description));
                    break;
                case SortOrder.CostAscending:
                    HintStatePerTeam.Sort((a, b) => a.Hint.Cost.CompareTo(b.Hint.Cost));
                    break;
                case SortOrder.CostDescending:
                    HintStatePerTeam.Sort((a, b) => -a.Hint.Cost.CompareTo(b.Hint.Cost));
                    break;
                case SortOrder.TimeAscending:
                    HintStatePerTeam.Sort((a, b) => a.UnlockTime.Value.CompareTo(b.UnlockTime.Value));
                    break;
                case SortOrder.TimeDescending:
                    HintStatePerTeam.Sort((a, b) => -a.UnlockTime.Value.CompareTo(b.UnlockTime.Value));
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