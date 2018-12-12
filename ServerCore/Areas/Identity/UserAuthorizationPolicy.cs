using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity
{
    public class IsGlobalAdminRequirement : IAuthorizationRequirement
    {
        public IsGlobalAdminRequirement()
        {
        }
    }

    public class IsGlobalAdminHandler : AuthorizationHandler<IsGlobalAdminRequirement>
    {
        private readonly PuzzleServerContext puzzleContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsGlobalAdminHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            puzzleContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       IsGlobalAdminRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);

            if (puzzleUser.IsGlobalAdmin)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

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

    public class AuthorizationHelper
    {
        public static Event GetEventFromContext(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext filterContext)
            {
                string eventIdAsString = filterContext.RouteData.Values["eventId"] as string;

                if (Int32.TryParse(eventIdAsString, out int eventId))
                {
                    PuzzleServerContext puzzleServerContext = (PuzzleServerContext)filterContext.HttpContext.RequestServices.GetService(typeof(PuzzleServerContext));
                    return puzzleServerContext.Events.Where(e => e.ID == eventId).FirstOrDefault();
                }
            }

            return null;
        }
    }
}

