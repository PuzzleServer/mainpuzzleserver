using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class FullDetailsModel : EventSpecificPageModel
    {
        public FullDetailsModel(PuzzleServerContext context, UserManager<IdentityUser> manager) : base(context, manager)
        {
        }

        public Team Team { get; set; }
        public bool HasTeam { get; set; }
        public IList<TeamMembers> Members { get; set; }
        public string Emails { get; set; }
        public IList<Tuple<PuzzleUser, int>> Users { get; set; }

        public async Task<IActionResult> OnGetAsync(int teamId = -1)
        {
            HasTeam = false;
            if (EventRole == ModelBases.EventRole.play)
            {
                // Ignore reqeusted team IDs for players - always re-direct to their own team
                Team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            }
            else if (teamId == -1)
            {
                return NotFound("Missing team id");
            }
            else
            {
                Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            }

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
            if (EventRole == EventRole.play && !Event.IsTeamRegistrationActive)
            {
                return NotFound("Membership changes are not open.");
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
            return RedirectToPage("./Members", new { teamId = teamId });
        }

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
            return RedirectToPage("./FullDetails", new { teamId = teamId });
        }
    }
}
