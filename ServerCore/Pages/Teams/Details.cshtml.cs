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
    public class DetailsModel : EventSpecificPageModel
    {
        public DetailsModel(PuzzleServerContext context, UserManager<IdentityUser> manager) : base(context, manager)
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
            return RedirectToPage("./Details", new { teamId = teamId });
        }

        public async Task<IActionResult> OnGetAddMemberAsync(int teamId, int userId, int applicationId)
        {
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
    }
}
