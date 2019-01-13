﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is registered for the event in the route.
    /// </summary>
    public class IsPlayerInEventRequirement : IAuthorizationRequirement
    {
        public IsPlayerInEventRequirement()
        {
        }
    }

    public class IsPlayerInEventHandler : AuthorizationHandler<IsPlayerInEventRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsPlayerInEventHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsPlayerInEventRequirement requirement)
        {
            await AuthorizationHelper.IsEventPlayerCheck(authContext, dbContext, userManager, requirement);
        }
    }
}
