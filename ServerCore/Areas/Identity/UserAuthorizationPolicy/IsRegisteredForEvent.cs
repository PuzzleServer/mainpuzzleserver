using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Requires that the current user is part of the event in the route - can be as an admin, author or player.
    /// </summary>
    public class IsRegisteredForEventRequirement : IAuthorizationRequirement
    {
    }

    public class IsRegisteredForEventHandler_Admin : AuthorizationHandler<IsRegisteredForEventRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsRegisteredForEventHandler_Admin(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsRegisteredForEventRequirement requirement)
        {
            await authHelper.IsEventAdminCheck(authContext, requirement);
        }
    }

    public class IsRegisteredForEventHandler_Author : AuthorizationHandler<IsRegisteredForEventRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsRegisteredForEventHandler_Author(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsRegisteredForEventRequirement requirement)
        {
            await authHelper.IsEventAuthorCheck(authContext, requirement);
        }
    }

    public class IsRegisteredForEventHandler_Player : AuthorizationHandler<IsRegisteredForEventRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsRegisteredForEventHandler_Player(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsRegisteredForEventRequirement requirement)
        {
            await authHelper.IsPlayerRegisteredForEvent(authContext, requirement);
        }
    }
}
