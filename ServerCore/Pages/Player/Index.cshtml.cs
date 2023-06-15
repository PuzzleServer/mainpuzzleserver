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

        public IList<PlayerInEvent> PlayerInEvent { get;set; }

        public async Task OnGetAsync()
        {
            PlayerInEvent = await _context.PlayerInEvent
                .Where(p => p.EventId == Event.ID)
                .Include(p => p.Event)
                .Include(p => p.Player).ToListAsync();
        }
    }
}
