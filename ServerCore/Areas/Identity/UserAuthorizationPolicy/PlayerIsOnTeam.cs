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
        private readonly AuthorizationHelper authHelper;

        public PlayerIsOnTeamHandler(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       PlayerIsOnTeamRequirement requirement)
        {
            await authHelper.IsPlayerOnTeamCheck(authContext, requirement);
        }
    }
}
   