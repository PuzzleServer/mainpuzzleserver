using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class TeamListBase : EventSpecificPageModel
    {
        public TeamListBase(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        /// <summary>
        /// All teams
        /// </summary>
        public Dictionary<Team, int> Teams { get; set; }

        /// <summary>
        /// Count of all players on teams
        /// </summary>
        public int PlayerCount { get; set; }

        protected async Task LoadTeamDataAsync()
        {
            List<Team> allTeams = null;
            allTeams = await (from team in _context.Teams
                              where team.Event == Event
                              select team).ToListAsync();
            Teams = await (from team in _context.Teams
                           where team.Event == Event
                           join teamMember in _context.TeamMembers on team equals teamMember.Team
                           group teamMember by teamMember.Team into teamCounts
                           select new { Team = teamCounts.Key, Count = teamCounts.Count() }).ToDictionaryAsync(x => x.Team, x => x.Count);

            foreach (Team team in allTeams)
            {
                if (!Teams.ContainsKey(team))
                {
                    Teams[team] = 0;
                }
                else
                {
                    PlayerCount += Teams[team];
                }
            }
        }
    }
}
