using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;

namespace ServerCore.Pages.Teams
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]    
    public class IndexModel : TeamListBase
    {
        public IndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadTeamDataAsync(false);
            return Page();
        }
    }
}
