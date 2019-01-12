using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Requires that the current user is part of the event in the route - can be as an admin, author or player.
    /// </summary>
    public class IsRegisteredForEventRequirement : IAuthorizationRequirement
    {
    }

    public class IsRegisteredForEventHandler_Admin : AuthorizationHandler<IsRegisteredForEventRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsRegisteredForEventHandler_Admin(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsRegisteredForEventRequirement requirement)
        {
            await AuthorizationHelper.IsEventAdminCheck(authContext, dbContext, userManager, requirement);
        }
    }

    public class IsRegisteredForEventHandler_Author : AuthorizationHandler<IsRegisteredForEventRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsRegisteredForEventHandler_Author(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsRegisteredForEventRequirement requirement)
        {
            await AuthorizationHelper.IsEventAuthorCheck(authContext, dbContext, userManager, requirement);
        }
    }

    public class IsRegisteredForEventHandler_Player : AuthorizationHandler<IsRegisteredForEventRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsRegisteredForEventHandler_Player(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsRegisteredForEventRequirement requirement)
        {
            await AuthorizationHelper.IsEventPlayerCheck(authContext, dbContext, userManager, requirement);
        }
    }
}
