using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is an admin for the event in the route.
    /// </summary>
    public class IsAdminInEventRequirement : IAuthorizationRequirement
    {
        public IsAdminInEventRequirement()
        {
        }
    }

    public class IsAdminInEventHandler : AuthorizationHandler<IsAdminInEventRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsAdminInEventHandler(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsAdminInEventRequirement requirement)
        {
            await authHelper.IsEventAdminCheck(authContext, requirement);
        }
    }
}
