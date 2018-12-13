using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    public class IsAuthorOfPuzzleRequirement : IAuthorizationRequirement
    {
        public IsAuthorOfPuzzleRequirement()
        {
        }
    }

    public class IsAuthorOfPuzzleHandler : AuthorizationHandler<IsAuthorOfPuzzleRequirement>
    {
        private readonly PuzzleServerContext puzzleContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsAuthorOfPuzzleHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            puzzleContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       IsAuthorOfPuzzleRequirement requirement)
        {
            //PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);
            //Puzzle puzzle = puzzleContext.Puzzles.Where(p => p.ID == requirement.PuzzleId).FirstOrDefault();
            //Event thisEvent = AuthorizationHelper.GetEventFromContext(context);

            ////  if (thisEvent != null && UserEventHelper.IsAuthorOfPuzzle(puzzleContext, puzzle, puzzleUser))
            //{
            //    context.Succeed(requirement);
            //}

            return Task.CompletedTask;
        }
    }
}