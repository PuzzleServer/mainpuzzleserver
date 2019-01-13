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
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsAuthorInEventHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsAuthorInEventRequirement requirement)
        {
            await AuthorizationHelper.IsEventAuthorCheck(authContext, dbContext, userManager, requirement);
        }
    }
}
