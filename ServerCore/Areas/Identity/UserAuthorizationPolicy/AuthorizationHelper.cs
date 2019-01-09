using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Areas.Identity
{
    public class AuthorizationHelper
    {
        public static async Task<Event> GetEventFromContext(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext filterContext)
            {
                string eventIdAsString = filterContext.RouteData.Values["eventId"] as string;

                if (Int32.TryParse(eventIdAsString, out int eventId))
                {
                    PuzzleServerContext puzzleServerContext = (PuzzleServerContext)filterContext.HttpContext.RequestServices.GetService(typeof(PuzzleServerContext));
                    return await puzzleServerContext.Events.Where(e => e.ID == eventId).FirstOrDefaultAsync();
                }
            }

            return null;
        }

        public static async Task<Puzzle> GetPuzzleFromContext(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext filterContext)
            {
                string puzzleIdAsString = filterContext.RouteData.Values["puzzleId"] as string;

                if (Int32.TryParse(puzzleIdAsString, out int puzzleId))
                {
                    PuzzleServerContext puzzleServerContext = (PuzzleServerContext)filterContext.HttpContext.RequestServices.GetService(typeof(PuzzleServerContext));
                    return await puzzleServerContext.Puzzles.Where(e => e.ID == puzzleId).FirstOrDefaultAsync();
                }
            }

            return null;
        }

        public static async Task<Team> GetTeamFromContext(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext filterContext)
            {
                string teamIdAsString = filterContext.RouteData.Values["teamId"] as string;

                if (Int32.TryParse(teamIdAsString, out int teamId))
                {
                    PuzzleServerContext puzzleServerContext = (PuzzleServerContext)filterContext.HttpContext.RequestServices.GetService(typeof(PuzzleServerContext));
                    return await puzzleServerContext.Teams.Where(e => e.ID == teamId).FirstOrDefaultAsync();
                }
            }

            return null;
        }

        public static EventRole GetEventRoleFromContext(AuthorizationHandlerContext context)
        {
            if (context.Resource is AuthorizationFilterContext filterContext)
            {
                string eventRole = filterContext.RouteData.Values["eventRole"] as string;

                Enum.TryParse<EventRole>(eventRole, out EventRole role);
                return role;
            }

            return EventRole.play;
        }

        public static async Task IsEventAdminCheck(AuthorizationHandlerContext authContext, PuzzleServerContext dbContext, UserManager<IdentityUser> userManager, IAuthorizationRequirement requirement)
        {
            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Event thisEvent = await AuthorizationHelper.GetEventFromContext(authContext);
            EventRole role = AuthorizationHelper.GetEventRoleFromContext(authContext);

            if (thisEvent != null && role == EventRole.admin && await puzzleUser.IsAdminForEvent(dbContext, thisEvent))
            {
                authContext.Succeed(requirement);
            }
        }

        public static async Task IsPuzzleAuthorCheck(AuthorizationHandlerContext authContext, PuzzleServerContext dbContext, UserManager<IdentityUser> userManager, IAuthorizationRequirement requirement)
        {
            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Puzzle puzzle = await AuthorizationHelper.GetPuzzleFromContext(authContext);
            Event thisEvent = await AuthorizationHelper.GetEventFromContext(authContext);
            EventRole role = AuthorizationHelper.GetEventRoleFromContext(authContext);

            if (thisEvent != null && role == EventRole.author && await UserEventHelper.IsAuthorOfPuzzle(dbContext, puzzle, puzzleUser))
            {
                authContext.Succeed(requirement);
            }
        }

        public static async Task IsEventAuthorCheck(AuthorizationHandlerContext authContext, PuzzleServerContext dbContext, UserManager<IdentityUser> userManager, IAuthorizationRequirement requirement)
        {
            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            EventRole role = AuthorizationHelper.GetEventRoleFromContext(authContext);

            if (authContext.Resource is AuthorizationFilterContext filterContext)
            {
                Event thisEvent = await AuthorizationHelper.GetEventFromContext(authContext);

                if (thisEvent != null && role == EventRole.author && await puzzleUser.IsAuthorForEvent(dbContext, thisEvent))
                {
                    authContext.Succeed(requirement);
                }
            }
        }

        public static async Task IsEventPlayerCheck(AuthorizationHandlerContext authContext, PuzzleServerContext dbContext, UserManager<IdentityUser> userManager, IAuthorizationRequirement requirement)
        {
            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Event thisEvent = await AuthorizationHelper.GetEventFromContext(authContext);
            EventRole role = AuthorizationHelper.GetEventRoleFromContext(authContext);

            if (thisEvent != null && role == EventRole.play && await puzzleUser.IsPlayerInEvent(dbContext, thisEvent))
            {
                authContext.Succeed(requirement);
            }
        }
    }
}

