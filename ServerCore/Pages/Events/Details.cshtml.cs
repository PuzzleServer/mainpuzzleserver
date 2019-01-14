using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsEventAdmin")]
    public class DetailsModel : EventSpecificPageModel
    {
        public DetailsModel(PuzzleServerContext context, UserManager<IdentityUser> userManager) : base(context, userManager)
        {
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}
