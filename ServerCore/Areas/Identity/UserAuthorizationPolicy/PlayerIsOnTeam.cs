using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    public class PlayerIsOnTeamRequirement : IAuthorizationRequirement
    {
        public PlayerIsOnTeamRequirement()
        {
        }
    }

    public class PlayerIsOnTeamHandler : AuthorizationHandler<PlayerIsOnTeamRequirement>
    {
        private readonly PuzzleServerContext puzzleContext;
        private readonly UserManager<IdentityUser> userManager;

        public PlayerIsOnTeamHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            puzzleContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       PlayerIsOnTeamRequirement requirement)
        {
            //PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);
            //Puzzle puzzle = puzzleContext.Puzzles.Where(p => p.ID == requirement.TeamId).FirstOrDefault();
            //Event thisEvent = AuthorizationHelper.GetEventFromContext(context);

            //if (thisEvent != null && UserEventHelper.GetTeamForPlayer(puzzleContext, thisEvent, puzzleUser).Result.ID == requirement.TeamId)
            //{
            //    context.Succeed(requirement);
            //}

            return Task.CompletedTask;
        }
    }
}
   