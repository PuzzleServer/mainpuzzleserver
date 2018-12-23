using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class PlayersModel : EventSpecificPageModel
    {
        public IList<TeamMembers> Players { get; set; }

        public string Emails { get; set; }

        public PlayersModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(int teamId)
        {
            Players = await _context.TeamMembers
                .Where(member => member.Team.Event == Event)
                .ToListAsync();

            StringBuilder emailList = new StringBuilder("");
            foreach (TeamMembers Player in Players)
            {
                emailList.Append(Player.Member.Email + "; ");
            }
            Emails = emailList.ToString();

            return Page();
        }
    }
}