using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    public class PlayerIsOnTeamRequirement : IAuthorizationRequirement
    {
        public int TeamId { get; set; }

        public PlayerIsOnTeamRequirement(int teamId)
        {
            TeamId = teamId;
        }
    }

    public class PlayerIsOnTeamHandler : AuthorizationHandler<PlayerIsOnTeamRequirement>
    {
        private readonly PuzzleServerContext puzzleContext;
        private readonly UserManager<IdentityUser> userManager;

        public PlayerIsOnTeamHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            puzzleContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       PlayerIsOnTeamRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);
            Puzzle puzzle = puzzleContext.Puzzles.Where(p => p.ID == requirement.TeamId).FirstOrDefault();
            Event thisEvent = AuthorizationHelper.GetEventFromContext(context);

            if (thisEvent != null && UserEventHelper.GetTeamForPlayer(puzzleContext, thisEvent, puzzleUser).Result.ID == requirement.TeamId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    internal class PlayerIsOnTeamAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "PlayerIsOnTeam";

        public int TeamId
        {
            get
            {
                if (Int32.TryParse(Policy.Substring(POLICY_PREFIX.Length), out int teamId))
                {
                    return teamId;
                }
                return default(Int32);
            }
            set
            {
                Policy = $"{POLICY_PREFIX}{value.ToString()}";
            }
        }

        public PlayerIsOnTeamAttribute(int teamId)
        {
            TeamId = teamId;
        }
    }

    internal class PlayerIsOnTeamPolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "PlayerIsOnTeam";

        public DefaultAuthorizationPolicyProvider DefaultPolicyProvider { get; }

        // Required fallback for cases when authorization attribute is created this specific policy
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return DefaultPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase) &&
                        int.TryParse(policyName.Substring(POLICY_PREFIX.Length), out int teamId))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PlayerIsOnTeamRequirement(teamId));
                return Task.FromResult(policy.Build());
            }

            return Task.FromResult<AuthorizationPolicy>(null);
        }
    }
}

