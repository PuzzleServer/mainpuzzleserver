using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Models;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class AddMemberModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public Team MyTeam { get; set; }

        public IList<User> Users { get; set; }

        public AddMemberModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int teamId)
        {
            MyTeam = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (MyTeam == null)
            {
                return NotFound("Could not find team with ID '" + teamId + "'. Check to make sure you're accessing this page in the context of a team.");
            }

            Users = await _context.Users.ToListAsync();
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

            User user = await _context.Users.FirstOrDefaultAsync(m => m.ID == userId);
            if (user == null)
            {
                return NotFound("Could not find user with ID '" + userId + "'. Check to make sure the user hasn't been removed.");
            }
            Member.Member = user;

            _context.TeamMembers.Add(Member);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Members", new { id = teamId });

            //SqlException: Cannot insert duplicate key row in object 'dbo.TeamMembers' with unique index 'IX_TeamMembers_Team.ID'. The duplicate key value is (2).
        }

        // TEMPORARY - can't test member add without this function
        // TODO - delete once users have an add page
        public async Task<IActionResult> OnGetAddUserAsync(int teamId)
        {
            User User = new User();
            User.Name = "MyName" + new Random().Next(1, 99);
            User.EmployeeAlias = "null";
            User.EmailAddress = User.Name + "@domain.com";
            User.PhoneNumber = "null";
            User.TShirtSize = "null";
            User.VisibleToOthers = true;
            _context.Users.Add(User);

            await _context.SaveChangesAsync();
            return RedirectToPage("./AddMember", new { teamId });
        }
    }
}