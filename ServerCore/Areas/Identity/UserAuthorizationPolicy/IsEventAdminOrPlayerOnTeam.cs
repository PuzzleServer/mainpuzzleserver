using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is an admin or author for the event in the route.
    /// </summary>
    public class IsEventAdminOrPlayerOnTeamRequirement : IAuthorizationRequirement
    {
    }

    public class IsEventAdminOrPlayerOnTeamHandler_Admin : AuthorizationHandler<IsEventAdminOrPlayerOnTeamRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsEventAdminOrPlayerOnTeamHandler_Admin(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, IsEventAdminOrPlayerOnTeamRequirement requirement)
        {
            await AuthorizationHelper.IsEventAdminCheck(authContext, dbContext, userManager, requirement);
        }
    }

    public class IsEventAdminOrPlayerOnTeamHandler_Play : AuthorizationHandler<IsEventAdminOrPlayerOnTeamRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsEventAdminOrPlayerOnTeamHandler_Play(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext, IsEventAdminOrPlayerOnTeamRequirement requirement)
        {
            await AuthorizationHelper.IsPlayerOnTeamCheck(authContext, dbContext, userManager, requirement);
        }
    }
}
