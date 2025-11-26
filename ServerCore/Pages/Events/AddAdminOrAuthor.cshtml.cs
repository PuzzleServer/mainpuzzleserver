using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    [Authorize("IsEventAdmin")]
    public class AddAdminOrAuthorModel : EventSpecificPageModel
    {
        public bool addAdmin { get; set; }

        public AddAdminOrAuthorModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(bool addAdmin)
        {
            this.addAdmin = addAdmin;

            return Page();
        }
    }
}