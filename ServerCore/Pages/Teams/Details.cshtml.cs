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
        public bool HasTeam { get; set; }

        public async Task<IActionResult> OnGetAsync(int id=-1)
        {
            HasTeam = false;
            if (EventRole == ModelBases.EventRole.play)
            {
                // Ignore reqeusted team IDs for players - always re-direct to their own team
                Team = await UserEventHelper.GetTeamForPlayer(context, Event, LoggedInUser);
            }
            else if (id == -1)
            {
                return NotFound("Missing team id");
            }
            else
            {
                Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == id);
            }

            if (Team == null)
            {
                if (EventRole != ModelBases.EventRole.play)
                {
                    return NotFound("No team found with id '" + id + "'.");
                }
                // The html page handles the 'no team' case using HasTeam
                return Page();
            }
            HasTeam = true;
            return Page();
        }
    }
}
