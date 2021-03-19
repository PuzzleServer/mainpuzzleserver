using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly AuthorizationHelper authHelper;

        public IsEventAdminOrEventAuthorHandler_Admin(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, IsEventAdminOrEventAuthorRequirement requirement)
        {
            await authHelper.IsEventAdminCheck(authContext, requirement);
        }
    }

    public class IsEventAdminOrEventAuthorHandler_Author : AuthorizationHandler<IsEventAdminOrEventAuthorRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsEventAdminOrEventAuthorHandler_Author(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, IsEventAdminOrEventAuthorRequirement requirement)
        {
            await authHelper.IsEventAuthorCheck(authContext, requirement);
        }
    }
}
