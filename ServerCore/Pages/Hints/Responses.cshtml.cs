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
                    .Select(h => new HintView { PuzzleId = h.Puzzle.ID, PuzzleName = h.Puzzle.PlaintextName, Description = h.Description, Content=h.Content, Cost = h.Cost })
                    .ToListAsync();
            }
            else
            {
                HintViews = await (from p in UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                                   join h in _context.Hints on p equals h.Puzzle
                                   select new HintView { PuzzleId = p.ID, PuzzleName = p.PlaintextName, Description = h.Description, Content = h.Content, Cost = h.Cost })
                                   .ToListAsync();
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
                    HintViews.Sort((a, b) => Math.Abs(a.Cost).CompareTo(Math.Abs(b.Cost)));
                    break;
                case SortOrder.CostDescending:
                    HintViews.Sort((a, b) => -Math.Abs(a.Cost).CompareTo(Math.Abs(b.Cost)));
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