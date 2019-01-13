using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is an admin for the event in the route or the author of the puzzle in the route.
    /// </summary>
    public class IsEventAdminOrAuthorOfPuzzleRequirement : IAuthorizationRequirement
    {
        public IsEventAdminOrAuthorOfPuzzleRequirement()
        {
        }
    }

    public class IsEventAdminOrAuthorOfPuzzleHandler_Admin : AuthorizationHandler<IsEventAdminOrAuthorOfPuzzleRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsEventAdminOrAuthorOfPuzzleHandler_Admin(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsEventAdminOrAuthorOfPuzzleRequirement requirement)
        {
            await AuthorizationHelper.IsEventAdminCheck(authContext, dbContext, userManager, requirement);
        }
    }

    public class IsEventAdminOrAuthorOfPuzzleHandler_Author : AuthorizationHandler<IsEventAdminOrAuthorOfPuzzleRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsEventAdminOrAuthorOfPuzzleHandler_Author(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsEventAdminOrAuthorOfPuzzleRequirement requirement)
        {
            await AuthorizationHelper.IsPuzzleAuthorCheck(authContext, dbContext, userManager, requirement);
        }
    }
}
