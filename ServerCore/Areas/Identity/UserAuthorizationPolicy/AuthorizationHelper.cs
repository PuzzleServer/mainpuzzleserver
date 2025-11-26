using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ServerCore.Areas.Identity.UserAuthorizationPolicy;
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

            return EventRole.Parse(eventRole);
        }

        public async Task IsPuzzleAdminCheck(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {
            EventRole role = GetEventRoleFromRoute();
            if (role != EventRole.admin)
            {
                return;
            }

            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Puzzle puzzle = await GetPuzzleFromRoute();
            Event thisEvent = await GetEventFromRoute();

            if (thisEvent != null && await UserEventHelper.IsAdminOfPuzzle(dbContext, puzzle, puzzleUser))
            {
                authContext.Succeed(requirement);
            }

            if (puzzle != null)
            {
                dbContext.Entry(puzzle).State = EntityState.Detached;
            }
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

            if (thisEvent != null && puzzleUser != null && await puzzleUser.IsAdminForEvent(dbContext, thisEvent))
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

        public async Task IsPlayerRegisteredForEvent(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {
            EventRole role = GetEventRoleFromRoute();
            if (role != EventRole.play)
            {
                return;
            }

            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Event thisEvent = await GetEventFromRoute();

            if (thisEvent != null && await puzzleUser.IsRegisteredForEvent(dbContext, thisEvent))
            {
                authContext.Succeed(requirement);
            }
        }

        public async Task IsEventPlayerOnTeamCheck(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {
            EventRole role = GetEventRoleFromRoute();
            if (role != EventRole.play)
            {
                return;
            }

            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Event thisEvent = await GetEventFromRoute();

            if (thisEvent != null && await puzzleUser.IsPlayerOnTeam(dbContext, thisEvent))
            {
                authContext.Succeed(requirement);
            }
        }

        public async Task IsPlayerOnTeamCheck(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {
            EventRole role = GetEventRoleFromRoute();

            if (role != EventRole.play && role.Type != EventRoleType.impersonate)
            {
                return;
            }

            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            Team team = await GetTeamFromRoute();
            Event thisEvent = await GetEventFromRoute();

            if (thisEvent != null)
            {
                if (role.Type == EventRoleType.impersonate)
                {
                    if (team.EventID == thisEvent.ID && await puzzleUser.IsAdminForEvent(dbContext, thisEvent))
                    {
                        authContext.Succeed(requirement);
                    }
                }
                else
                {
                    Team userTeam = await UserEventHelper.GetTeamForPlayer(dbContext, thisEvent, puzzleUser);
                    if (userTeam != null && userTeam.ID == team.ID)
                    {
                        authContext.Succeed(requirement);
                    }
                }
            }
        }

        public async Task PlayerCanSeePuzzleCheck(AuthorizationHandlerContext authContext, IAuthorizationRequirement requirement)
        {
            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            if (puzzleUser == null) { return; }

            Puzzle puzzle = await GetPuzzleFromRoute();
            Event thisEvent = await GetEventFromRoute();

            if (thisEvent != null && puzzle != null)
            {
                if (thisEvent.AreAnswersAvailableNow || thisEvent.IsAlphaTestingEvent)
                {
                    authContext.Succeed(requirement);
                }
                else if (puzzle.IsForSinglePlayer)
                {
                    IQueryable<SinglePlayerPuzzleUnlockState> statesQ = SinglePlayerPuzzleUnlockStateHelper.GetFullReadOnlyQuery(dbContext, thisEvent, puzzle.ID);

                    if (statesQ.FirstOrDefault()?.UnlockedTime != null)
                    {
                        authContext.Succeed(requirement);
                    }
                    else
                    {
                        IQueryable<SinglePlayerPuzzleStatePerPlayer> statePerPlayerQ = SinglePlayerPuzzleStateHelper.GetFullReadOnlyQuery(dbContext, thisEvent, puzzle.ID, puzzleUser.ID);
                        if (statePerPlayerQ.FirstOrDefault()?.UnlockedTime != null)
                        {
                            authContext.Succeed(requirement);
                        }
                    }
                }
                else
                {
                    EventRole role = GetEventRoleFromRoute();
                    Team team = null;
                    if (role.Type == EventRoleType.impersonate)
                    {
                        if (await puzzleUser.IsAdminForEvent(dbContext, thisEvent) || await UserEventHelper.IsAuthorOfPuzzle(dbContext, puzzle, puzzleUser))
                        {
                            team = await dbContext.Teams.Where(t => t.ID == role.ImpersonatingTeamId).FirstOrDefaultAsync();
                        }
                    }
                    else
                    {
                        team = await UserEventHelper.GetTeamForPlayer(dbContext, thisEvent, puzzleUser);
                    }

                    if (team != null)
                    {
                        IQueryable<PuzzleStatePerTeam> statesQ = PuzzleStateHelper.GetFullReadOnlyQuery(dbContext, thisEvent, puzzle, team);

                        if (statesQ.FirstOrDefault()?.UnlockedTime != null)
                        {
                            authContext.Succeed(requirement);
                        }
                    }
                }
            }
        }

        internal async Task IsMicrosoftOrCommunityCheck(AuthorizationHandlerContext authContext, IsMicrosoftOrCommunityRequirement requirement)
        {
            var msaUser = await userManager.GetUserAsync(authContext.User);
            if (msaUser?.Email?.EndsWith("@microsoft.com") == true)
            {
                // Allow Microsoft employees
                authContext.Succeed(requirement);
                return;
            }

            PuzzleUser puzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(dbContext, authContext.User, userManager);
            if (puzzleUser == null)
            {
                return;
            }

            // Allow admins and authors of any event as part of the community
            if (puzzleUser.MayBeAdminOrAuthor)
            {
                authContext.Succeed(requirement);
            }
        }
    }
}

