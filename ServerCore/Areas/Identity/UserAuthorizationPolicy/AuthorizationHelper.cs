using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Areas.Identity
{
    public class AuthorizationHelper
    {
        PuzzleServerContext dbContext { get; }
        UserManager<IdentityUser> userManager { get; }
        IHttpContextAccessor httpContextAccessor { get; }

        public AuthorizationHelper(PuzzleServerContext puzzleServerContext, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            dbContext = puzzleServerContext;
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        private async Task<Event> GetEventFromRoute()
        {
            RouteValueDictionary route = httpContextAccessor.HttpContext.Request.RouteValues;
            Event result = null;
            string eventId = route["eventId"] as string;

            result = await EventHelper.GetEventFromEventId(dbContext, eventId);
            return result;
        }

        private async Task<Puzzle> GetPuzzleFromRoute()
        {
            RouteValueDictionary route = httpContextAccessor.HttpContext.Request.RouteValues;
            string puzzleIdAsString = route["puzzleId"] as string;

            if (Int32.TryParse(puzzleIdAsString, out int puzzleId))
            {
                return await dbContext.Puzzles.Where(e => e.ID == puzzleId).FirstOrDefaultAsync();
            }

            return null;
        }

        private async Task<Team> GetTeamFromRoute()
        {
            RouteValueDictionary route = httpContextAccessor.HttpContext.Request.RouteValues;
            string teamIdAsString = route["teamId"] as string;

            if (Int32.TryParse(teamIdAsString, out int teamId))
            {
                return await dbContext.Teams.Where(e => e.ID == teamId).FirstOrDefaultAsync();
            }

            return null;
        }

        private EventRole GetEventRoleFromRoute()
        {
            RouteValueDictionary route = httpContextAccessor.HttpContext.Request.RouteValues;
            string eventRole = route["eventRole"] as string;

            if (Enum.TryParse<EventRole>(eventRole, out EventRole role))
            {
                return role;
            }

            return EventRole.play;
        }

        public async Task IsEventAdminCheck(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {            
            EventRole role = GetEventRoleFromRoute();
            if (role != EventRole.admin)
            {
                return;
            }

            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Event thisEvent = await GetEventFromRoute();

            if (thisEvent != null && await puzzleUser.IsAdminForEvent(dbContext, thisEvent))
            {
                authContext.Succeed(requirement);
            }
        }

        public async Task IsPuzzleAuthorCheck(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {
            EventRole role = GetEventRoleFromRoute();
            if (role != EventRole.author)
            {
                return;
            }

            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Puzzle puzzle = await GetPuzzleFromRoute();
            Event thisEvent = await GetEventFromRoute();

            if (thisEvent != null && await UserEventHelper.IsAuthorOfPuzzle(dbContext, puzzle, puzzleUser))
            {
                authContext.Succeed(requirement);
            }

            if (puzzle != null)
            {
                dbContext.Entry(puzzle).State = EntityState.Detached;
            }
        }

        public async Task IsEventAuthorCheck(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {
            EventRole role = GetEventRoleFromRoute();
            if (role != EventRole.author)
            {
                return;
            }

            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);

            Event thisEvent = await GetEventFromRoute();

            if (thisEvent != null && await puzzleUser.IsAuthorForEvent(dbContext, thisEvent))
            {
                authContext.Succeed(requirement);
            }
        }

        public async Task IsEventPlayerCheck(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {
            EventRole role = GetEventRoleFromRoute();
            if (role != EventRole.play)
            {
                return;
            }

            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Event thisEvent = await GetEventFromRoute();

            if (thisEvent != null && await puzzleUser.IsPlayerInEvent(dbContext, thisEvent))
            {
                authContext.Succeed(requirement);
            }
        }

        public async Task IsPlayerOnTeamCheck(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {
            EventRole role = GetEventRoleFromRoute();

            if (role != EventRole.play)
            {
                return;
            }

            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Team team = await GetTeamFromRoute();
            Event thisEvent = await GetEventFromRoute();

            if (thisEvent != null)
            {
                Team userTeam = await UserEventHelper.GetTeamForPlayer(dbContext, thisEvent, puzzleUser);
                if (userTeam != null && userTeam.ID == team.ID)
                {
                    authContext.Succeed(requirement);
                }
            }
        }

        public async Task PlayerCanSeePuzzleCheck(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {
            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Puzzle puzzle = await GetPuzzleFromRoute();
            Event thisEvent = await GetEventFromRoute();

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

