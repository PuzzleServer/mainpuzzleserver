using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class NotificationsModel : EventSpecificPageModel
    {
        public NotificationsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public IActionResult OnGet(int teamId)
        {
            return Page();
        }
    }
}
