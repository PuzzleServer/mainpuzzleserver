using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [AllowAnonymous]
    public class ApplyModel : EventSpecificPageModel
    {

        public ApplyModel(PuzzleServerContext context, UserManager<IdentityUser> userManager)
            : base(context, userManager)
        {
        }

        public IActionResult OnGet(int teamID)
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }
            
            return Page();
        }
    }
}