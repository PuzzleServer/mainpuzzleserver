using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    public class IsAdminInEventRequirement : IAuthorizationRequirement
    {
        public IsAdminInEventRequirement()
        {
        }
    }

    public class IsAdminInEventHandler : AuthorizationHandler<IsAdminInEventRequirement>
    {
        private readonly PuzzleServerContext puzzleContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsAdminInEventHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            puzzleContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       IsAdminInEventRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);

            Event thisEvent = AuthorizationHelper.GetEventFromContext(context);

            if (thisEvent != null && puzzleUser.IsAdminForEvent(puzzleContext, thisEvent))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
