using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class DetailsModel : EventSpecificPageModel
    {
        public DetailsModel(PuzzleServerContext context, UserManager<IdentityUser> manager) : base(context, manager)
        {
        }

        public Team Team { get; set; }

        public async Task<IActionResult> OnGetAsync(int id=-1)
        {
            if (id == -1)
            {
                if (EventRole != ModelBases.EventRole.play)
                {
                    return NotFound("Missing team id");
                }
                id = 1;// TODO - fix to get the user's team loggedInUser.teamId;
            }
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == id);

            if (Team == null)
            {
                return NotFound("No team found with id '" + id + "'.");
            }
            return Page();
        }
    }
}
