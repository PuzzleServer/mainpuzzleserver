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
    [Authorize("IsEventAdminOrPlayerOnTeam")]
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

            if (EventRole == EventRole.play)
            {
                // Get all users that want to be on this team
                Users = await (from application in _context.TeamApplications
                               where application.Team == Team &&
                               !((from teamMember in _context.TeamMembers
                                  where teamMember.Member == application.Player &&
                             teamMember.Team.Event == Event
                                  select teamMember).Any())
                               select new Tuple<PuzzleUser, int>(application.Player, application.ID)).ToListAsync();
            }
            else
            {
                Debug.Assert(EventRole == EventRole.admin);

                // Admins can add anyone
                Users = await (from user in _context.PuzzleUsers
                               where !((from teamMember in _context.TeamMembers
                                        where teamMember.Team.Event == Event
                                        select teamMember).Any())
                               select new Tuple<PuzzleUser, int>(user, -1)).ToListAsync();
            }

            return Page();
        }

        [BindProperty]
        public TeamMembers Member { get; set; }

        public async Task<IActionResult> OnGetAddMemberAsync(int teamId, int userId, int applicationId)
        {
            if (applicationId == -1)
            {
                if (EventRole != EventRole.admin)
                {
                    return Forbid();
                }
            }

            if (EventRole == EventRole.play && !Event.IsTeamMembershipChangeActive)
            {
                return NotFound("Team membership change is not currently active.");
            }

            TeamMembers Member = new TeamMembers();

            Team team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (team == null)
            {
                return NotFound("Could not find team with ID '" + teamId + "'. Check to make sure the team hasn't been removed.");
            }
            Member.Team = team;

            PuzzleUser user = await _context.PuzzleUsers.FirstOrDefaultAsync(m => m.ID == userId);
            if (user == null)
            {
                return NotFound("Could not find user with ID '" + userId + "'. Check to make sure the user hasn't been removed.");
            }
            Member.Member = user;

            if (await (from teamMember in _context.TeamMembers
                       where teamMember.Member == user &&
                       teamMember.Team.Event == Event
                       select teamMember).AnyAsync())
            {
                return NotFound("User is already on a team in this event.");
            }

            if (applicationId != -1)
            {
                TeamApplication application = await (from app in _context.TeamApplications
                                               where app.ID == applicationId
                                               select app).FirstOrDefaultAsync();
                if (application == null)
                {
                    return NotFound("Could not find application");
                }

                if (application.Player.ID != userId)
                {
                    return NotFound("Mismatched player and application");
                }

                if (application.Team != team)
                {
                    return Forbid();
                }
            }

            // Remove any applications the user might have started for this event
            var allApplications = from app in _context.TeamApplications
                                  where app.Player == user &&
                                  app.Team.Event == Event
                                  select app;
            _context.TeamApplications.RemoveRange(allApplications);

            _context.TeamMembers.Add(Member);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Members", new { teamId = teamId });
        }
    }
}