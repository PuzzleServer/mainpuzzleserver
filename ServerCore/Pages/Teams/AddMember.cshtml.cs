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
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [Authorize("IsEventAdmin")]
    public class AddMemberModel : EventSpecificPageModel
    {
        public Team Team { get; set; }

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

            return Page();
        }
    }
}