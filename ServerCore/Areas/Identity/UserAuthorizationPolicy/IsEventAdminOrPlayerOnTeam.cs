using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is an admin or author for the event in the route.
    /// </summary>
    public class IsEventAdminOrPlayerOnTeamRequirement : IAuthorizationRequirement
    {
    }

    public class IsEventAdminOrPlayerOnTeamHandler_Admin : AuthorizationHandler<IsEventAdminOrPlayerOnTeamRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsEventAdminOrPlayerOnTeamHandler_Admin(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, IsEventAdminOrPlayerOnTeamRequirement requirement)
        {
            await authHelper.IsEventAdminCheck(authContext, requirement);
        }
    }

    public class IsEventAdminOrPlayerOnTeamHandler_Play : AuthorizationHandler<IsEventAdminOrPlayerOnTeamRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsEventAdminOrPlayerOnTeamHandler_Play(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, IsEventAdminOrPlayerOnTeamRequirement requirement)
        {
            await authHelper.IsPlayerOnTeamCheck(authContext, requirement);
        }
    }
}
