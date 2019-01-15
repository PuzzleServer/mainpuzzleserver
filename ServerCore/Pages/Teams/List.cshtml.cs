using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    /// <summary>
    /// Player-facing list of all teams
    /// </summary>
    [AllowAnonymous]
    public class ListModel : EventSpecificPageModel
    {
        public ListModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        /// <summary>
        /// All teams
        /// </summary>
        public Dictionary<Team, int> Teams { get;set; }

        /// <summary>
        /// True if the current user is on a team
        /// </summary>
        public bool UserOnTeam { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if (await LoggedInUser.IsPlayerInEvent(_context, Event))
            {
                UserOnTeam = true;
            }
            else
            {
                UserOnTeam = false;
            }

            //Teams = await _context.Teams.ToListAsync();
            List<Team> allTeams = await (from team in _context.Teams
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
            }

            return Page();
        }
    }
}
