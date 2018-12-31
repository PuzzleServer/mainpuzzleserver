using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is the author of the puzzle in the route.
    /// </summary>
    public class IsAuthorOfPuzzleRequirement : IAuthorizationRequirement
    {
        public IsAuthorOfPuzzleRequirement()
        {
        }
    }

    public class IsAuthorOfPuzzleHandler : AuthorizationHandler<IsAuthorOfPuzzleRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsAuthorOfPuzzleHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsAuthorOfPuzzleRequirement requirement)
        {
            return AuthorizationHelper.IsPuzzleAuthorCheck(authContext, dbContext, userManager, requirement);
        }
    }
}