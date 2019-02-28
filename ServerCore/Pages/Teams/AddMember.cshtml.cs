using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [Authorize("IsEventAdmin")]
    public class AddMemberModel : EventSpecificPageModel
    {
        public Team Team { get; set; }

        public IList<Tuple<PuzzleUser, int>> Users { get; set; }

        public AddMemberModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(int teamId)
        {
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (Team == null)
            {
                return NotFound("Could not find team with ID '" + teamId + "'. Check to make sure you're accessing this page in the context of a team.");
            }

            Users = await (from user in _context.PuzzleUsers
                            where !((from teamMember in _context.TeamMembers
                                    where teamMember.Team.Event == Event
                                    where teamMember.Member == user
                                    select teamMember).Any())
                            select new Tuple<PuzzleUser, int>(user, -1)).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetAddMemberAsync(int teamId, int userId, int applicationId)
        {
            Tuple<bool, string> result = TeamHelper.AddMemberAsync(_context, Event, EventRole, teamId, userId, applicationId).Result;
            if (result.Item1)
            {
                return RedirectToPage("./Details", new { teamId = teamId });
            }
            return NotFound(result.Item2);
        }
    }
}