using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.ModelBases
{
    public abstract class EventSpecificPageModel : PageModel
    {
        [FromRoute]
        [ModelBinder(typeof(EventBinder))]
        public Event Event { get; set; }

        [FromRoute]
        [ModelBinder(typeof(RoleBinder))]
        public EventRole EventRole { get; set; }

        public int? Refresh { get; set; }

        /// <summary>
        /// Runs before page actions
        /// </summary>
        public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            LoggedInUser = await PuzzleUser.GetPuzzleUserForCurrentUser(_context, User, userManager);

            // Required to have the rest of page execution occur
            await next.Invoke();
        }

        public PuzzleUser LoggedInUser { get; private set; }

        protected readonly PuzzleServerContext _context;
        private readonly UserManager<IdentityUser> userManager;

        public EventSpecificPageModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager)
        {
            _context = serverContext;
            userManager = manager;
        }

        private bool? isRegisteredUser;
        public async Task<bool> IsRegisteredUser()
        {
            if (LoggedInUser == null)
            {
                return false;
            }

            if (isRegisteredUser == null)
            {
                isRegisteredUser = await LoggedInUser.IsRegisteredForEvent(_context, Event);
            }

            return isRegisteredUser.Value;
        }

        private bool? playerIsOnTeam;
        public async Task<bool> PlayerHasTeamForEvent()
        {
            if(LoggedInUser == null) { return false; }

            if(playerIsOnTeam == null)
            {
                playerIsOnTeam = await LoggedInUser.IsPlayerOnTeam(_context, Event);
            }

            return playerIsOnTeam.Value;
        }

        /// <summary>
        /// Checks whether or not it is an intern event and that the logged in user is allowed in it
        /// </summary>
        /// <returns>True if it is an intern event and the logged in user is allowed in</returns>
        public bool IsNotAllowedInInternEvent()
        {
            return Event.IsInternEvent
                && TeamHelper.IsMicrosoftNonIntern(LoggedInUser.Email)
                && EventRole != EventRole.admin;
        }

        private bool? isEventAuthor;
        public async Task<bool> IsEventAuthor()
        {
            if (LoggedInUser == null)
            {
                return false;
            }

            if (isEventAuthor == null)
            {
                isEventAuthor = await LoggedInUser.IsAuthorForEvent(_context, Event);
            }

            return isEventAuthor.Value;
        }

        private bool? isEventAdmin;
        public async Task<bool> IsEventAdmin()
        {
            if (LoggedInUser == null)
            {
                return false;
            }

            if (isEventAdmin == null)
            {
                isEventAdmin = await LoggedInUser.IsAdminForEvent(_context, Event);
            }

            return isEventAdmin.Value;
        }

        /// <summary>
        ///  Whether or not the individual has filled out their swag request
        /// </summary>
        public async Task<bool> HasSwag()
        {
            return Event.HasSwag && await _context.PlayerInEvent.Where(m => m.Event == Event && m.Player == LoggedInUser).AnyAsync();
        }

        /// <summary>
        /// Whether or not the event is providing swag (i.e. shirt & lunch)
        /// </summary>
        public bool EventHasSwag()
        {
            return Event.HasSwag;
        }

        public async Task<int> GetTeamId()
        {
            if (EventRole == ModelBases.EventRole.play)
            {
                Team team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
                return team != null ? team.ID : -1;
            }
            else
            {
                return -1;
            }
        }

        public async Task<bool> GetShowTeamAnnouncement()
        {
            if (EventRole == ModelBases.EventRole.play)
            {
                Team team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
                return team != null ? team.ShowTeamAnnouncement : false;
            }
            else
            {
                return false;
            }
        }

        public async Task<int> GetPlayerEventId()
        {
            if (EventRole == EventRole.play)
            {
                PlayerInEvent player = await UserEventHelper.GetPlayerInEvent(_context, Event, LoggedInUser);
                return player != null ? player.ID : -1;
            }
            return -1;
        }

        public string LocalTime(DateTime? date)
        {
            return TimeHelper.LocalTime(date);
        }

        public class EventBinder : IModelBinder
        {
            public async Task BindModelAsync(ModelBindingContext bindingContext)
            {
                string eventId = bindingContext.ActionContext.RouteData.Values["eventId"] as string;

                PuzzleServerContext puzzleServerContext = bindingContext.HttpContext.RequestServices.GetService<PuzzleServerContext>();

                Event eventObj = await EventHelper.GetEventFromEventId(puzzleServerContext, eventId);

                if (eventObj != null)
                {
                    bindingContext.Result = ModelBindingResult.Success(eventObj);
                }
            }
        }

        public class RoleBinder : IModelBinder
        {
            // This doesn't actually run async but the compiler complains if I try to use BindModel :(
            public async Task BindModelAsync(ModelBindingContext bindingContext)
            {
                string eventRoleAsString = bindingContext.ActionContext.RouteData.Values["eventRole"] as string;
                if (eventRoleAsString == null)
                {
                    eventRoleAsString = ModelBases.EventRole.play.ToString();
                }
                // TODO: Add auth check to make sure the user has permissions for the given eventRole
                eventRoleAsString = eventRoleAsString.ToLower();

                if (Enum.IsDefined(typeof(EventRole), eventRoleAsString))
                {
                    bindingContext.Result = ModelBindingResult.Success(Enum.Parse(typeof(EventRole), eventRoleAsString));
                }
                else
                {
                    throw new Exception("Invalid route parameter '" + eventRoleAsString + "'. Please check your URL to make sure you are using the correct path. (code: InvalidRoleId)");
                }
            }
        }
    }
}
