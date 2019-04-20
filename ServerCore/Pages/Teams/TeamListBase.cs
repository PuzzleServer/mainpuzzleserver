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

            PlayerCountByTeamID = await
                _context.Teams.Where(team => team.Event == Event)
                              .Join(_context.TeamMembers, team => team.ID, teamMember => teamMember.Team.ID, (team, teamMember) => team.ID)
                              .GroupBy(x => x)
                              .Select(x => new { TeamID = x.Key, Count = x.Count() })
                              .ToDictionaryAsync(x => x.TeamID, x => x.Count);

            // Get a list of all teams in alphabetical order by team name.

            Teams = await (from team in _context.Teams
                           where team.Event == Event
                           orderby team.Name
                           select team).ToListAsync();

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
