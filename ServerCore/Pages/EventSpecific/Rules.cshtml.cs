using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.EventSpecific
{
    public class RulesModel : EventSpecificPageModel
    {
        public RulesModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager): base(serverContext, userManager)
        {

        }
    }
}