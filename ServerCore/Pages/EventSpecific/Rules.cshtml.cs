using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.EventSpecific
{
    [AllowAnonymous]
    public class RulesModel : EventSpecificPageModel
    {
        public RulesModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager): base(serverContext, userManager)
        {
            pageType = "rules";
        }
    }
}