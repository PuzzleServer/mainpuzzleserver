﻿using System.Threading.Tasks;
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
        private readonly AuthorizationHelper authHelper;

        public IsAuthorOfPuzzleHandler(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       IsAuthorOfPuzzleRequirement requirement)
        {
            await authHelper.IsPuzzleAuthorCheck(authContext, requirement);
        }
    }
}