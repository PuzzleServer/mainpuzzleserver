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
        public bool IsMicrosoft { get; set; }

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

            int existingTeamId = await GetTeamId();
            if (EventRole == EventRole.play && existingTeamId != -1)
            {
                return RedirectToPage("/Teams/Details", new { teamId = existingTeamId });
            }

            if (LoggedInUser.Email.EndsWith("@microsoft.com"))
            {
                IsMicrosoft = true;
            }

            return Page();
        }
    }
}
