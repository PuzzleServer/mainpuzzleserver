using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

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

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       PlayerIsOnTeamRequirement requirement)
        {
            await AuthorizationHelper.IsPlayerOnTeamCheck(authContext, dbContext, userManager, requirement);
        }
    }
}
   