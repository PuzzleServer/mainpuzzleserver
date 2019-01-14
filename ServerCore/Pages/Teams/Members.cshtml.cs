using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [Authorize("IsEventAdminOrPlayerOnTeam")]
    public class MembersModel : EventSpecificPageModel
    {
        public MembersModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public Team Team { get; set; }

        public IList<TeamMembers> Members { get; set; }

        public string Emails { get; set; }

        public async Task<IActionResult> OnGetAsync(int teamId)
        {
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);

            if (Team == null)
            {
                return NotFound("Could not find team with ID '" + teamId + "'. Check to make sure you're accessing this page in the context of a team.");
            }
            
            Members = await _context.TeamMembers.Where(members => members.Team.ID == Team.ID).ToListAsync();
            
            StringBuilder emailList = new StringBuilder("");
            foreach (TeamMembers Member in Members)
            {
                emailList.Append(Member.Member.Email + "; ");
            }
            Emails = emailList.ToString();

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
    }
}