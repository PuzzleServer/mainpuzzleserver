using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    public class PlayerCanSeePuzzleRequirement : IAuthorizationRequirement
    {
        public int PuzzleId { get; set; }

        public PlayerCanSeePuzzleRequirement(int puzzleId)
        {
            PuzzleId = puzzleId;
        }
    }

    public class PlayerCanSeePuzzleHandler : AuthorizationHandler<PlayerCanSeePuzzleRequirement>
    {
        private readonly PuzzleServerContext puzzleContext;
        private readonly UserManager<IdentityUser> userManager;

        public PlayerCanSeePuzzleHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            puzzleContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       PlayerCanSeePuzzleRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);
            Puzzle puzzle = puzzleContext.Puzzles.Where(p => p.ID == requirement.PuzzleId).FirstOrDefault();
            Event thisEvent = AuthorizationHelper.GetEventFromContext(context);

            if (thisEvent != null && puzzleUser.IsPlayerInEvent(puzzleContext, thisEvent))
            {
                Team team = UserEventHelper.GetTeamForPlayer(puzzleContext, thisEvent, puzzleUser).Result;
                IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper.GetFullReadOnlyQuery(puzzleContext, thisEvent, puzzle, team);
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    internal class PlayerCanSeePuzzleAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "PlayerCanSeePuzzle";

        public int PuzzleId
        {
            get
            {
                if (Int32.TryParse(Policy.Substring(POLICY_PREFIX.Length), out int puzzleId))
                {
                    return puzzleId;
                }
                return default(Int32);
            }
            set
            {
                Policy = $"{POLICY_PREFIX}{value.ToString()}";
            }
        }

        public PlayerCanSeePuzzleAttribute(int puzzleId)
        {
            PuzzleId = puzzleId;
        }
    }

    internal class PlayerCanSeePuzzlePolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "PlayerCanSeePuzzle";

        public DefaultAuthorizationPolicyProvider DefaultPolicyProvider { get; }

        // Required fallback for cases when authorization attribute is created this specific policy
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return DefaultPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase) &&
                        int.TryParse(policyName.Substring(POLICY_PREFIX.Length), out int puzzleId))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PlayerCanSeePuzzleRequirement(puzzleId));
                return Task.FromResult(policy.Build());
            }

            return Task.FromResult<AuthorizationPolicy>(null);
        }
    }
}
