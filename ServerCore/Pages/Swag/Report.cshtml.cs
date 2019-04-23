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
using System.ComponentModel.DataAnnotations;

namespace ServerCore.Pages.Swag
{
    /// <summary>
    /// Model for the author/admin's view of the feedback items for a specific puzzle
    /// /used for viewing and aggregating feedback for a specific puzzle
    /// </summary>
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class SwagReportModel : EventSpecificPageModel
    {
        public SwagReportModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public class SwagView
        {
            public string PlayerName { get; set; }
            public string TeamName { get; set; }
            public string Lunch { get; set; }
            public string LunchModifications { get; set; }
            public string ShirtSize { get; set; }
        }

        public List<SwagView> SwagViews { get; set; }

        public async Task<IActionResult> OnGetAsync(int? puzzleId)
        {
            SwagViews = await (from s in _context.Swag
                                   join t in _context.TeamMembers on s.PlayerId equals t.Member.ID
                                   where s.Event == Event && t.Team.Event == Event
                                   orderby t.Team.Name, s.ShirtSize
                                   select new SwagView()
                                   {
                                       PlayerName = t.Member.Name,
                                       TeamName = t.Team.Name,
                                       Lunch = s.Lunch,
                                       LunchModifications = s.LunchModifications,
                                       ShirtSize = s.ShirtSize
                                   }).ToListAsync();
            return Page();
        }
    }
}
