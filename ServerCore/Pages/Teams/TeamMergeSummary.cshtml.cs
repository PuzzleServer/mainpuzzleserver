using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [Authorize(Policy = "IsEventAdmin")]
    public class TeamMergeSummaryModel : EventSpecificPageModel
    {
        public TeamMergeSummaryModel(
            PuzzleServerContext serverContext,
            UserManager<IdentityUser> userManager)
            : base(serverContext, userManager)
        {
        }

        public class TeamSummary{
            public TeamSummary(int teamId, string teamName, string mergedTeams)
            {
                TeamID = teamId;
                TeamName = teamName;
                MergedTeams = mergedTeams; 
            }

            public int TeamID { get; }
            public string TeamName { get; set; }
            public string MergedTeams { get; set; }
        }

        public IEnumerable<TeamSummary> TeamSummaries { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            TeamSummaries = await (from team in _context.Teams
                                   where team.Event == Event && team.MergedTeams != null
                                   orderby team.Name
                                   select team)
                .Select(t => new TeamSummary(t.ID, t.Name, t.MergedTeams))
                .ToListAsync();
            return Page();
        }
    }
}
