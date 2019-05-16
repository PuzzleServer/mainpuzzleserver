using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    // Includes redirects which requires custom auth - always call the AuthChecks method & check the passed value at the start of any method
    [AllowAnonymous]
    public class DetailsModel : EventSpecificPageModel
    {
        public DetailsModel(PuzzleServerContext context, UserManager<IdentityUser> manager) : base(context, manager)
        {
        }

        public Team Team { get; set; }
        public IList<TeamMembers> Members { get; set; }
        public string Emails { get; set; }
        public IList<Tuple<PuzzleUser, int>> Users { get; set; }

        private async Task<(bool passed, IActionResult redirect)> AuthChecks(int teamId)
        {
            // Force the user to log in
            if (LoggedInUser == null)
            {
                return (false, Challenge());
            }

            //Only allow admins OR players who are on the team)
            int playerTeamId = await GetTeamId();
            if (!((EventRole == EventRole.admin && await IsEventAdmin()) || (EventRole == EventRole.play && playerTeamId == teamId)))
            {
                // Redirect players to the relevant page
                if (EventRole == EventRole.play)
                {
                    if (playerTeamId != -1)
                    {
                        return (false, RedirectToPage("./Details", new { teamId = playerTeamId }));
                    }
                    return (false, RedirectToPage("./Apply", new { teamId = teamId }));
                }
                // Everyone else gets an error
                return (false, NotFound("You do not have permission to view the details of this team."));
            }
            return (true, null);
        }

        public async Task<IActionResult> OnGetAsync(int teamId = -1)
        {
            var authResult = await AuthChecks(teamId);
            if (!authResult.passed)
            {
                return authResult.redirect;
            }

            if (teamId == -1)
            {
                return NotFound("Missing team id");
            }

            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (Team == null)
            {
                return NotFound("No team found with id '" + teamId + "'.");
            }

            // Existing team members
            Members = await _context.TeamMembers.Where(members => members.Team.ID == Team.ID).ToListAsync();
            StringBuilder emailList = new StringBuilder("");
            foreach (TeamMembers Member in Members)
            {
                emailList.Append(Member.Member.Email + "; ");
            }
            Emails = emailList.ToString();

            // Team applicants
            Users = await (from application in _context.TeamApplications
                           where application.Team == Team &&
                           !((from teamMember in _context.TeamMembers
                              where teamMember.Member == application.Player &&
                         teamMember.Team.Event == Event
                              select teamMember).Any())
                           select new Tuple<PuzzleUser, int>(application.Player, application.ID)).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetRemoveMemberAsync(int teamId, int teamMemberId)
        {
            var authResult = await AuthChecks(teamId);
            if (!authResult.passed)
            {
                return authResult.redirect;
            }

            if (EventRole == EventRole.play && !Event.IsTeamMembershipChangeActive)
            {
                return NotFound("Team membership change is not currently active.");
            }

            TeamMembers member = await _context.TeamMembers.FirstOrDefaultAsync(m => m.ID == teamMemberId && m.Team.ID == teamId);
            if (member == null)
            {
                return NotFound("Could not find team member with ID '" + teamMemberId + "'. They may have already been removed from the team.");
            }

            if (EventRole == EventRole.play)
            {
                int teamCount = await (from count in _context.TeamMembers
                                       where count.Team.ID == teamId
                                       select count).CountAsync();
                if (teamCount == 1)
                {
                    return NotFound("Cannot remove the last member of a team. Delete the team instead.");
                }
            }

            _context.TeamMembers.Remove(member);
            await _context.SaveChangesAsync();
            
            if (EventRole == EventRole.play && member.Member == LoggedInUser)
            {
                return RedirectToPage("./List");
            }
            return RedirectToPage("./Details", new { teamId = teamId });
        }

        public async Task<IActionResult> OnGetAddMemberAsync(int teamId, int userId, int applicationId)
        {
            var authResult = await AuthChecks(teamId);
            if (!authResult.passed)
            {
                return authResult.redirect;
            }

            if (EventRole == EventRole.play && !Event.IsTeamMembershipChangeActive)
            {
                return NotFound("Team membership change is not currently active.");
            }

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
            Team team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (application.Team != team)
            {
                return Forbid();
            }

            Tuple<bool, string> result = TeamHelper.AddMemberAsync(_context, Event, EventRole, teamId, userId).Result;
            if (result.Item1)
            {
                return RedirectToPage("./Details", new { teamId = teamId });
            }
            return NotFound(result.Item2);
        }

        public async Task<IActionResult> OnGetRejectMemberAsync(int teamId, int userId, int applicationId)
        {
            var authResult = await AuthChecks(teamId);
            if (!authResult.passed)
            {
                return authResult.redirect;
            }

            if (EventRole == EventRole.play && !Event.IsTeamMembershipChangeActive)
            {
                return NotFound("Team membership change is not currently active.");
            }

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
            Team team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (application.Team != team)
            {
                return Forbid();
            }

            var allApplications = from app in _context.TeamApplications
                                  where app.Player == application.Player &&
                                  app.Team.Event == Event
                                  select app;
            _context.TeamApplications.RemoveRange(allApplications);
            await _context.SaveChangesAsync();

            MailHelper.Singleton.SendPlaintextWithoutBcc(new string[] { team.PrimaryContactEmail, application.Player.Email },
                $"{Event.Name}: Your application was rejected from {team.Name}",
                $"Sorry! You can apply to another team if you wish.");

            return RedirectToPage("./Details", new { teamId = teamId });
        }
    }
}
