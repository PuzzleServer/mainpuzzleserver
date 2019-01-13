using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is an admin or author for the event in the route.
    /// </summary>
    public class IsEventAdminOrEventAuthorRequirement : IAuthorizationRequirement
    {
    }

    public class IsEventAdminOrEventAuthorHandler_Admin : AuthorizationHandler<IsEventAdminOrEventAuthorRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsEventAdminOrEventAuthorHandler_Admin(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, IsEventAdminOrEventAuthorRequirement requirement)
        {
            await AuthorizationHelper.IsEventAdminCheck(authContext, dbContext, userManager, requirement);
        }
    }

    public class IsEventAdminOrEventAuthorHandler_Author : AuthorizationHandler<IsEventAdminOrEventAuthorRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsEventAdminOrEventAuthorHandler_Author(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, IsEventAdminOrEventAuthorRequirement requirement)
        {
            await AuthorizationHelper.IsPuzzleAuthorCheck(authContext, dbContext, userManager, requirement);
        }
    }
}
