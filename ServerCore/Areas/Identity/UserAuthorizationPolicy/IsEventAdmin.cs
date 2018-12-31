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
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsAdminInEventHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsAdminInEventRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);

            Event thisEvent = AuthorizationHelper.GetEventFromContext(authContext);

            if (thisEvent != null && puzzleUser.IsAdminForEvent(dbContext, thisEvent).Result)
            {
                authContext.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
