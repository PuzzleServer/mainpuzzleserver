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
        public List<Team> Teams { get; set; }

        /// <summary>
        /// For each team ID, its player count
        /// </summary>
        public Dictionary<int, int> PlayerCountByTeamID { get; set; }

        /// <summary>
        /// Count of all players on teams
        /// </summary>
        public int PlayerCount { get; set; }

        protected async Task LoadTeamDataAsync()
        {
            // Join the teams table with the players table to get a dictionary mapping team IDs to player counts.
            // Launch this asynchronously so we can overlap it with the next query.

            Task<Dictionary<int, int>> PlayerCountByTeamIDTask =
                (from team in _context.Teams
                 where team.Event == Event
                 join teamMember in _context.TeamMembers on team equals teamMember.Team
                 group teamMember by teamMember.Team into teamCounts
                 select new { TeamID = teamCounts.Key.ID, Count = teamCounts.Count() }
                ).ToDictionaryAsync(x => x.TeamID, x => x.Count);

            // Get a list of all teams in alphabetical order by team name.

            Teams = await (from team in _context.Teams
                           where team.Event == Event
                           orderby team.Name
                           select team).ToListAsync();

            // Now, wait for the asynchronous dictionary-construction task we launched earlier to complete.

            PlayerCountByTeamID = await PlayerCountByTeamIDTask;

            // Loop through all the teams to accomplish two things:
            //
            // (1) Make sure that the PlayerCountByTeamID dictionary has every team ID in Teams among its keys.
            // (2) Add up all the player counts to get PlayerCount.

            foreach (Team team in Teams)
            {
                if (!PlayerCountByTeamID.ContainsKey(team.ID))
                {
                    PlayerCountByTeamID[team.ID] = 0;
                }
                else
                {
                    PlayerCount += PlayerCountByTeamID[team.ID];
                }
            }
        }
    }
}
