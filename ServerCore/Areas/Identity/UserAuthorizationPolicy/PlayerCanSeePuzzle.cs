using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Areas.Identity.UserAuthorizationPolicy
{
    /// <summary>
    /// Require that the current user has permission to see the puzzle in the route.
    /// </summary>
    public class PlayerCanSeePuzzleRequirement : IAuthorizationRequirement
    {
        public PlayerCanSeePuzzleRequirement()
        {
        }
    }

    public class PlayerCanSeePuzzleHandler : AuthorizationHandler<PlayerCanSeePuzzleRequirement>
    {
        private readonly PuzzleServerContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        public PlayerCanSeePuzzleHandler(PuzzleServerContext pContext, UserManager<IdentityUser> manager)
        {
            dbContext = pContext;
            userManager = manager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
                                                       PlayerCanSeePuzzleRequirement requirement)
        {
            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Puzzle puzzle = await AuthorizationHelper.GetPuzzleFromContext(authContext);
            Event thisEvent = await AuthorizationHelper.GetEventFromContext(authContext);

            if (thisEvent != null && puzzle != null)
            {
                Team team = await UserEventHelper.GetTeamForPlayer(dbContext, thisEvent, puzzleUser);

                if (team != null)
                {
                    IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper.GetFullReadOnlyQuery(dbContext, thisEvent, puzzle, team);

                    if (statesQ.FirstOrDefault().UnlockedTime != null || thisEvent.AreAnswersAvailableNow)
                    {
                        authContext.Succeed(requirement);
                    }
                }
            }
        }
    }
}
