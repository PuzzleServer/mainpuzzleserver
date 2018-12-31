using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is on the team in the route.
    /// </summary>
    public class PlayerIsOnTeamRequirement : IAuthorizationRequirement
    {
    }

    public class PlayerIsOnTeamHandler : AuthorizationHandler<PlayerIsOnTeamRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public PlayerIsOnTeamHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       PlayerIsOnTeamRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Team team = AuthorizationHelper.GetTeamFromContext(authContext);
            Event thisEvent = AuthorizationHelper.GetEventFromContext(authContext);

            if (thisEvent != null && UserEventHelper.GetTeamForPlayer(dbContext, thisEvent, puzzleUser).Result.ID == team.ID)
            {
                authContext.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
   