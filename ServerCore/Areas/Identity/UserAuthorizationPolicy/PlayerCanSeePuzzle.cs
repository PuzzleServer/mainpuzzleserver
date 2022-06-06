using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user has permission to see the puzzle in the route.
    /// </summary>
    public class PlayerCanSeePuzzleRequirement : IAuthorizationRequirement
    {
        public PlayerCanSeePuzzleRequirement()
        {
        }
    }

    public class PlayerCanSeePuzzleHandler : AuthorizationHandler<PlayerCanSeePuzzleRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public PlayerCanSeePuzzleHandler(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       PlayerCanSeePuzzleRequirement requirement)
        {
            await authHelper.PlayerCanSeePuzzleCheck(authContext, requirement);
        }
    }
}
