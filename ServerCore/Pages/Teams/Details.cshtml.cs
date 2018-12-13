using System.Threading.Tasks;
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

        public async Task<IActionResult> OnGetAsync(int id=-1)
        {
            if (id == -1)
            {
                if (EventRole != ModelBases.EventRole.play)
                {
                    return NotFound("Missing team id");
                }
                Team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
                return NotFound("I tried. \nID:" + LoggedInUser.ID + " - IDID:" + LoggedInUser.IdentityUserId + " - EventID:" + Event.ID);// + " - TeamID:" + Team.ID);
            }
            else
            {
                Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == id);
            }

            if (Team == null)
            {
                return NotFound("No team found with id '" + id + "'.");
            }
            return Page();
        }
    }
}
