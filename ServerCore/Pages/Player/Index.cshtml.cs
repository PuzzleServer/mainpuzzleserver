using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Player
{
    [Authorize("IsEventAdmin")]
    public class IndexModel : EventSpecificPageModel
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public IList<PlayerWithTeamInEvent> PlayerInEvent { get;set; }

        public async Task OnGetAsync()
        {
            PlayerInEvent = await (from t in _context.Teams
                                   where t.EventID == Event.ID
                                   join tm in _context.TeamMembers on t.ID equals tm.Team.ID
                                   join p in _context.PlayerInEvent on tm.Member.ID equals p.Player.ID
                                   orderby p.Player.Name
                                   select new PlayerWithTeamInEvent { Player = p, Team = tm.Team }
                ).ToListAsync();
        }

        public class PlayerWithTeamInEvent {
            public PlayerInEvent Player { get; set; }
            public Team Team { get; set; }
        }
    }
}
