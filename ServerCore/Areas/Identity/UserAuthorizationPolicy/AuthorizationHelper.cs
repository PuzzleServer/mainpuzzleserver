using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using ServerCore.DataModel;

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
            if(context.Resource is AuthorizationFilterContext filterContext)
            {
                string puzzleIdAsString = filterContext.RouteData.Values["puzzleId"] as string;

                if(Int32.TryParse(puzzleIdAsString, out int puzzleId))
                {
                    PuzzleServerContext puzzleServerContext = (PuzzleServerContext)filterContext.HttpContext.RequestServices.GetService(typeof(PuzzleServerContext));
                    return puzzleServerContext.Puzzles.Where(e => e.ID == puzzleId).FirstOrDefault();
                }
            }

            return null;
        }
    }
}

