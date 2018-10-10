using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class MembersModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;
 
        public MembersModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        //[BindProperty]
        public Team Team { get; set; }

        public IList<User> Users { get; set; }

        public IList<TeamMembers> Members { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == id);

            if (Team == null)
            {
                return NotFound("Could not find team with ID '" + id + "'. Check to make sure you're accessing this page in the context of a team.");
            }
            
            Members = await _context.TeamMembers.Where(members => members.Team == Team).ToListAsync();

            Users = await _context.Users.ToListAsync();

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