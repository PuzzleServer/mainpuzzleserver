using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    public class IsAuthorOfPuzzleRequirement : IAuthorizationRequirement
    {
        public int PuzzleId { get; set; }

        public IsAuthorOfPuzzleRequirement(int puzzleId)
        {
            PuzzleId = puzzleId;
        }
    }

    public class IsAuthorOfPuzzleHandler : AuthorizationHandler<IsAuthorOfPuzzleRequirement>
    {
        private readonly PuzzleServerContext puzzleContext;
        private readonly UserManager<IdentityUser> userManager;

        public IsAuthorOfPuzzleHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            puzzleContext = pContext;
            userManager = manager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       IsAuthorOfPuzzleRequirement requirement)
        {
            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleContext, context.User, userManager);
            Puzzle puzzle = puzzleContext.Puzzles.Where(p => p.ID == requirement.PuzzleId).FirstOrDefault();
            Event thisEvent = AuthorizationHelper.GetEventFromContext(context);

            //  if (thisEvent != null && UserEventHelper.IsAuthorOfPuzzle(puzzleContext, puzzle, puzzleUser))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    internal class IsAuthorOfPuzzleAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "IsAuthorOfPuzzle";

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

        public IsAuthorOfPuzzleAttribute(int puzzleId)
        {
            PuzzleId = puzzleId;
        }
    }

    internal class IsAuthorOfPuzzlePolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "IsAuthorOfPuzzle";

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
                policy.AddRequirements(new IsAuthorOfPuzzleRequirement(puzzleId));
                return Task.FromResult(policy.Build());
            }

            return Task.FromResult<AuthorizationPolicy>(null);
        }
    }
}
