using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user is a Microsoft employee or an admin or author
    /// </summary>
    public class IsMicrosoftOrCommunityRequirement : IAuthorizationRequirement
    {
        public IsMicrosoftOrCommunityRequirement()
        {
        }
    }

    public class IsMicrosoftOrCommunityHandler : AuthorizationHandler<IsMicrosoftOrCommunityRequirement>
    {
        private readonly AuthorizationHelper authHelper;

        public IsMicrosoftOrCommunityHandler(AuthorizationHelper authHelper)
        {
            this.authHelper = authHelper;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                                  IsMicrosoftOrCommunityRequirement requirement)
        {
            await authHelper.IsMicrosoftOrCommunityCheck(authContext, requirement);
        }
    }
}
