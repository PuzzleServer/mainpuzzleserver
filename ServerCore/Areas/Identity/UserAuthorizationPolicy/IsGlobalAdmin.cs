using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require the current user is a global admin.
    /// </summary>
    public class IsGlobalAdminRequirement : IAuthorizationRequirement
    {
        public IsGlobalAdminRequirement()
        {
        }
    }

    public class IsGlobalAdminHandler : AuthorizationHandler<IsGlobalAdminRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsGlobalAdminHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsGlobalAdminRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);

            if (puzzleUser.IsGlobalAdmin)
            {
                authContext.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
