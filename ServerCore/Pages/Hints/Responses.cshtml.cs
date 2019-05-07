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
    public class ResponsesModel : EventSpecificPageModel
    {
        public ResponsesModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public List<HintView> HintViews { get; set; }

        public SortOrder? Sort { get; set; }

        public const SortOrder DefaultSort = SortOrder.PuzzleAscending;

        public async Task<IActionResult> OnGetAsync(SortOrder? sort)
        {
            Sort = sort;

            if (EventRole == EventRole.admin)
            {
                HintViews = await _context.Hints.Where(h => h.Puzzle.Event == Event)
                    .Select(h => new HintView { PuzzleId = h.Puzzle.ID, PuzzleName = h.Puzzle.Name, Description = h.Description, Content=h.Content, Cost = h.Cost })
                    .ToListAsync();
            }
            else
            {
                // Surely there is a way to get a join to do a bunch of this work, but joins are simply not for me. Someone else can fix later.
                var hintStatePerTeam = new List<HintStatePerTeam>();
                var authorPuzzleIDs = await UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser).Select(p => p.ID).ToListAsync();

                HintViews = await _context.Hints.Where(h => h.Puzzle.Event == Event)
                    .Select(h => new HintView { PuzzleId = h.Puzzle.ID, PuzzleName = h.Puzzle.Name, Description = h.Description, Content = h.Content, Cost = h.Cost })
                    .ToListAsync();
                HintViews = HintViews.Where((h) => authorPuzzleIDs.Contains(h.PuzzleId)).ToList();
            }

            switch (sort ?? DefaultSort)
            {
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
            public int PuzzleId { get; set; }
            public string PuzzleName { get; set; }
            public string Description { get; set; }
            public string Content { get; set; }
            public int Cost { get; set; }
        }

        public enum SortOrder
        {
            PuzzleAscending,
            PuzzleDescending,
            DescriptionAscending,
            DescriptionDescending,
            CostAscending,
            CostDescending
        }
    }
}