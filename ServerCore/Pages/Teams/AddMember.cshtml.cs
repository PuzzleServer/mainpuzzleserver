using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class AddMemberModel : EventSpecificPageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public Team Team { get; set; }

        public IList<PuzzleUser> Users { get; set; }

        public AddMemberModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int teamId)
        {
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (Team == null)
            {
                return NotFound("Could not find team with ID '" + teamId + "'. Check to make sure you're accessing this page in the context of a team.");
            }

            // Get all users that are not already on a team in this event
            Users = await _context.PuzzleUsers
                .Except(_context.TeamMembers
                .Where(member => member.Team.Event == Event)
                .Select(model => model.Member))
                .ToListAsync();
            
            return Page();
        }

        [BindProperty]
        public TeamMembers Member { get; set; }

        public async Task<IActionResult> OnGetAddMemberAsync(int teamId, int userId)
        {
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

            _context.TeamMembers.Add(Member);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Members", new { id = teamId });
        }
    }
}