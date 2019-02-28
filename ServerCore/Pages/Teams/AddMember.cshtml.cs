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
            Team team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (team == null)
            {
                return NotFound($"Could not find team with ID '{teamId}'. Check to make sure the team hasn't been removed.");
            }

            var currentTeamMembers = await _context.TeamMembers.Where(members => members.Team.ID == team.ID).ToListAsync();
            if (currentTeamMembers.Count >= Event.MaxTeamSize && EventRole != EventRole.admin)
            {
                return NotFound($"The team '{team.Name}' is full.");
            }

            PuzzleUser user = await _context.PuzzleUsers.FirstOrDefaultAsync(m => m.ID == userId);
            if (user == null)
            {
                return NotFound($"Could not find user with ID '{userId}'. Check to make sure the user hasn't been removed.");
            }

            if (user.EmployeeAlias == null && currentTeamMembers.Where((m) => m.Member.EmployeeAlias == null).Count() >= Event.MaxExternalsPerTeam)
            {
                return NotFound($"The team '{team.Name}' is already at its maximum count of non-employee players, and '{user.Email}' has no registered alias.");
            }

            if (await (from teamMember in _context.TeamMembers
                       where teamMember.Member == user &&
                       teamMember.Team.Event == Event
                       select teamMember).AnyAsync())
            {
                return NotFound($"'{user.Email}' is already on a team in this event.");
            }

            TeamMembers Member = new TeamMembers();
            Member.Team = team;
            Member.Member = user;

            // Remove any applications the user might have started for this event
            var allApplications = from app in _context.TeamApplications
                                  where app.Player == user &&
                                  app.Team.Event == Event
                                  select app;
            _context.TeamApplications.RemoveRange(allApplications);

            _context.TeamMembers.Add(Member);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Details", new { teamId = teamId });
        }
    }
}