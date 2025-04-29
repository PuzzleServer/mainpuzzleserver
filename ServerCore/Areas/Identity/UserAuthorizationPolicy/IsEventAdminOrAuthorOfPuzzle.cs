using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is an admin for the event in the route or the author of the puzzle in the route.
    /// </summary>
    public class IsEventAdminOrAuthorOfPuzzleRequirement : IAuthorizationRequirement
    {
        public IsEventAdminOrAuthorOfPuzzleRequirement()
        {
        }
    }

    public class IsEventAdminOrAuthorOfPuzzleHandler_Admin : AuthorizationHandler<IsEventAdminOrAuthorOfPuzzleRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsEventAdminOrAuthorOfPuzzleHandler_Admin(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsEventAdminOrAuthorOfPuzzleRequirement requirement)
        {
            await authHelper.IsPuzzleAdminCheck(authContext, requirement);
        }
    }

    public class IsEventAdminOrAuthorOfPuzzleHandler_Author : AuthorizationHandler<IsEventAdminOrAuthorOfPuzzleRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsEventAdminOrAuthorOfPuzzleHandler_Author(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsEventAdminOrAuthorOfPuzzleRequirement requirement)
        {
            await authHelper.IsPuzzleAuthorCheck(authContext, requirement);
        }
    }
}
