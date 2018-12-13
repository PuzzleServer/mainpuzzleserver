using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    public class IsAuthorInEventRequirement : IAuthorizationRequirement
    {
        public IsAuthorInEventRequirement()
        {
        }
    }

    public class IsAuthorInEventHandler : AuthorizationHandler<IsAuthorInEventRequirement>
    {
        private readonly PuzzleServerContext puzzleContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsAuthorInEventHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            puzzleContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       IsAuthorInEventRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);

            if (context.Resource is AuthorizationFilterContext filterContext)
            {
                Event thisEvent = AuthorizationHelper.GetEventFromContext(context);

                if (thisEvent != null && puzzleUser.IsAuthorForEvent(puzzleContext, thisEvent))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
