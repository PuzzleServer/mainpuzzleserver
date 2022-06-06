using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is an author for the event in the route.
    /// </summary>
    public class IsAuthorInEventRequirement : IAuthorizationRequirement
    {
        public IsAuthorInEventRequirement()
        {
        }
    }

    public class IsAuthorInEventHandler : AuthorizationHandler<IsAuthorInEventRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsAuthorInEventHandler(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsAuthorInEventRequirement requirement)
        {
            await authHelper.IsEventAuthorCheck(authContext, requirement);
        }
    }
}
