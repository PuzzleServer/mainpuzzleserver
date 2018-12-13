using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    public class IsPlayerInEventRequirement : IAuthorizationRequirement
    {
        public IsPlayerInEventRequirement()
        {
        }
    }

    public class IsPlayerInEventHandler : AuthorizationHandler<IsPlayerInEventRequirement>
    {
        private readonly PuzzleServerContext puzzleContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsPlayerInEventHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            puzzleContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       IsPlayerInEventRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);

            Event thisEvent = AuthorizationHelper.GetEventFromContext(context);

            if (thisEvent != null && puzzleUser.IsPlayerInEvent(puzzleContext, thisEvent))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
