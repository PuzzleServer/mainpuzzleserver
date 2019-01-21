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
        public IList<PuzzleUser> Admins { get; set; }

        public string AdminEmails { get; set; }

        public IList<PuzzleUser> Authors { get; set; }

        public string AuthorEmails { get; set; }

        public IList<TeamMembers> Players { get; set; }

        public string PlayerEmails { get; set; }

        public PlayersModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get admins

            Admins = await _context.EventAdmins
                .Where(admin => admin.Event == Event).Select(admin => admin.Admin)
                .ToListAsync();

            StringBuilder adminEmailList = new StringBuilder("");
            foreach (PuzzleUser admin in Admins)
            {
                adminEmailList.Append(admin.Email + "; ");
            }
            AdminEmails = adminEmailList.ToString();

            // Get authors

            Authors = await _context.EventAuthors
                .Where(author => author.Event == Event).Select(author => author.Author)
                .ToListAsync();

            StringBuilder authorEmailList = new StringBuilder("");
            foreach (PuzzleUser author in Authors)
            {
                authorEmailList.Append(author.Email + "; ");
            }
            AuthorEmails = authorEmailList.ToString();

            // Get players

            Players = await _context.TeamMembers
                .Where(member => member.Team.Event == Event)
                .ToListAsync();

            StringBuilder playerEmailList = new StringBuilder("");
            foreach (TeamMembers Player in Players)
            {
                playerEmailList.Append(Player.Member.Email + "; ");
            }
            PlayerEmails = playerEmailList.ToString();

            // Return page

            return Page();
        }
    }
}