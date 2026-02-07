using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using System.Linq;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public partial class JoinTeam
    {
        [Parameter]
        public Event Event { get; set; }

        [Parameter]
        public int LoggedInUserId { get; set; }

        [Parameter]
        public string EventRoleString { get; set; }

        [Inject]
        public PuzzleServerContext _context { get; set; }

        public List<Team> Teams { get; set; }

        public Dictionary<int, int> PlayerCountByTeamID { get; set; }

        int AppliedTeamId { get; set; }

        public EventRole EventRole;

        protected override async Task OnParametersSetAsync()
        {
            EventRole = EventRole.Parse(EventRoleString);

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

            var applications = from application in _context.TeamApplications
                               where application.PlayerID == LoggedInUserId && application.Team.Event == Event
                               select application.Team.ID;
            AppliedTeamId = await applications.FirstOrDefaultAsync();

            await base.OnParametersSetAsync();
        }
    }
}
