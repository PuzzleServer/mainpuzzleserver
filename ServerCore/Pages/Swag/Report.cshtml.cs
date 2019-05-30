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
    [Authorize(Policy = "IsEventAdmin")]
    public class SwagReportModel : EventSpecificPageModel
    {
        public SwagReportModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public class SwagView
        {
            public string PlayerName { get; set; }
            public string PlayerEmail { get; set; }
            public string TeamName { get; set; }
            public string Lunch { get; set; }
            public string LunchModifications { get; set; }
            public string ShirtSize { get; set; }
        }

        public List<SwagView> SwagViews { get; set; }

        public async Task<IActionResult> OnGetAsync(int? puzzleId)
        {
            SwagViews = await (from t in _context.TeamMembers
                               join s in _context.Swag on t.Member.ID equals s.PlayerId into tmp
                               from swagState in tmp.DefaultIfEmpty()
                               where (swagState == null || swagState.Event == Event) && t.Team.Event == Event
                               orderby t.Team.Name, swagState.ShirtSize
                               select new SwagView()
                               {
                                   PlayerName = t.Member.Name,
                                   PlayerEmail = t.Member.Email,
                                   TeamName = t.Team.Name,
                                   Lunch = swagState == null ? null : swagState.Lunch,
                                   LunchModifications = swagState == null ? null : swagState.LunchModifications,
                                   ShirtSize = swagState == null ? null : swagState.ShirtSize
                               }).ToListAsync();
            return Page();
        }
    }
}
