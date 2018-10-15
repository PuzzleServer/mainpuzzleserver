using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class IndexModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public IndexModel(PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Team> Team { get;set; }

        public async Task OnGetAsync()
        {
            Team = await _context.Teams.Where(team => team.Event == Event).ToListAsync();
        }
    }
}
