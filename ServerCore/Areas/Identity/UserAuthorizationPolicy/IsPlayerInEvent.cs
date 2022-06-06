using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is registered for the event in the route.
    /// </summary>
    public class IsPlayerInEventRequirement : IAuthorizationRequirement
    {
        public IsPlayerInEventRequirement()
        {
        }
    }

    public class IsPlayerInEventHandler : AuthorizationHandler<IsPlayerInEventRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsPlayerInEventHandler(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsPlayerInEventRequirement requirement)
        {
            await authHelper.IsEventPlayerCheck(authContext, requirement);
        }
    }
}
