using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class MembersModel : EventSpecificPageModel
    {
        public MembersModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public Team Team { get; set; }

        public IList<TeamMembers> Members { get; set; }

        public string Emails { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == id);

            if (Team == null)
            {
                return NotFound("Could not find team with ID '" + id + "'. Check to make sure you're accessing this page in the context of a team.");
            }
            
            Members = await _context.TeamMembers.Where(members => members.Team == Team).ToListAsync();
            
            StringBuilder emailList = new StringBuilder("");
            foreach (TeamMembers Member in Members)
            {
                //TODO - figure out why this isn't working: emailList.Append(Member.Member.Email + "; ");
            }
            Emails = emailList.ToString();

            return Page();
        }

        public async Task<IActionResult> OnGetRemoveMemberAsync(int teamId, int teamMemberId)
        {
            TeamMembers member = await _context.TeamMembers.FirstOrDefaultAsync(m => m.ID == teamMemberId);
            if (member == null)
            {
                return NotFound("Could not find team member with ID '" + teamMemberId + "'. They may have already been removed from the team.");
            }

            _context.TeamMembers.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Members", new { id = teamId });
        }
    }
}