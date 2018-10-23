using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class PlayersModel : EventSpecificPageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public IList<TeamMembers> Players { get; set; }

        public string Emails { get; set; }

        public PlayersModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int teamId)
        {
            Players = await _context.TeamMembers
                .Where(member => member.Team.Event == Event)
                .ToListAsync();

            Emails = "";
            foreach(TeamMembers Player in Players)
            {
                Emails += Player.Member.EmailAddress + "; ";
            }

            return Page();
        }
    }
}