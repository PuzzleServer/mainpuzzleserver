using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Areas.Identity
{
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

        public static Puzzle GetPuzzleFromContext(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext filterContext)
            {
                string puzzleIdAsString = filterContext.RouteData.Values["puzzleId"] as string;

                if (Int32.TryParse(puzzleIdAsString, out int puzzleId))
                {
                    PuzzleServerContext puzzleServerContext = (PuzzleServerContext)filterContext.HttpContext.RequestServices.GetService(typeof(PuzzleServerContext));
                    return puzzleServerContext.Puzzles.Where(e => e.ID == puzzleId).FirstOrDefault();
                }
            }

            return null;
        }

        public static Team GetTeamFromContext(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext filterContext)
            {
                string teamIdAsString = filterContext.RouteData.Values["teamId"] as string;

                if (Int32.TryParse(teamIdAsString, out int teamId))
                {
                    PuzzleServerContext puzzleServerContext = (PuzzleServerContext)filterContext.HttpContext.RequestServices.GetService(typeof(PuzzleServerContext));
                    return puzzleServerContext.Teams.Where(e => e.ID == teamId).FirstOrDefault();
                }
            }

            return null;
        }

        public static Task IsEventAdminCheck(AuthorizationHandlerContext authContext, PuzzleServerContext dbContext, UserManager<IdentityUser> userManager, IAuthorizationRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);

            Event thisEvent = AuthorizationHelper.GetEventFromContext(authContext);

            if (thisEvent != null && puzzleUser.IsAdminForEvent(dbContext, thisEvent).Result)
            {
                authContext.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        public static Task IsPuzzleAuthorCheck(AuthorizationHandlerContext authContext, PuzzleServerContext dbContext, UserManager<IdentityUser> userManager, IAuthorizationRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Puzzle puzzle = AuthorizationHelper.GetPuzzleFromContext(authContext);
            Event thisEvent = AuthorizationHelper.GetEventFromContext(authContext);

            if (thisEvent != null && UserEventHelper.IsAuthorOfPuzzle(dbContext, puzzle, puzzleUser).Result)
            {
                authContext.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        public static Task IsEventAuthorCheck(AuthorizationHandlerContext authContext, PuzzleServerContext dbContext, UserManager<IdentityUser> userManager, IAuthorizationRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);

            if (authContext.Resource is AuthorizationFilterContext filterContext)
            {
                Event thisEvent = AuthorizationHelper.GetEventFromContext(authContext);

                if (thisEvent != null && puzzleUser.IsAuthorForEvent(dbContext, thisEvent).Result)
                {
                    authContext.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }

        public static Task IsEventPlayerCheck(AuthorizationHandlerContext authContext, PuzzleServerContext dbContext, UserManager<IdentityUser> userManager, IAuthorizationRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);

            Event thisEvent = AuthorizationHelper.GetEventFromContext(authContext);

            if (thisEvent != null && puzzleUser.IsPlayerInEvent(dbContext, thisEvent).Result)
            {
                authContext.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

