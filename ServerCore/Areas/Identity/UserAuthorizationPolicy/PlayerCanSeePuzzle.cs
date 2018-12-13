using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    public class PlayerCanSeePuzzleRequirement : IAuthorizationRequirement
    {
        public PlayerCanSeePuzzleRequirement()
        {
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
            Puzzle puzzle = AuthorizationHelper.GetPuzzleFromContext(context);
            Event thisEvent = AuthorizationHelper.GetEventFromContext(context);

            if (thisEvent != null && puzzle != null)
            {
                Team team = UserEventHelper.GetTeamForPlayer(puzzleContext, thisEvent, puzzleUser).Result;

                if (team != null)
                {
                    IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper.GetFullReadOnlyQuery(puzzleContext, thisEvent, puzzle, team);

                    if (statesQ.FirstOrDefault().UnlockedTime != null || thisEvent.AreAnswersAvailableNow)
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
