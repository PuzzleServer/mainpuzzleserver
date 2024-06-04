using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServerCore.Pages.Resources
{
    [Authorize(Policy = "IsMicrosoftOrCommunity")]
    public class ArchiveModel : PageModel
    {
        public ActionResult OnGet()
        {
            return Redirect("https://puzzlearchive.azurewebsites.net/");
        }
    }
}
