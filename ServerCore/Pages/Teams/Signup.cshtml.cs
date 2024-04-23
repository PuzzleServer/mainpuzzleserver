using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCore.DataModel;
using ServerCore.ModelBases;
using System.Threading.Tasks;

namespace ServerCore.Pages.Teams
{
    public class SignupModel : EventSpecificPageModel
    {
        public SignupModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if ((EventRole != EventRole.play && EventRole != EventRole.admin)
                || IsNotAllowedInInternEvent()
                || (EventRole == EventRole.admin && !await LoggedInUser.IsAdminForEvent(_context, Event)))
            {
                return Forbid();
            }

            if (EventRole == EventRole.play && GetTeamId().Result != -1)
            {
                return NotFound("You are already on a team and cannot create a new one.");
            }

            if (!Event.IsTeamRegistrationActive && EventRole != EventRole.admin)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
