using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.ModelBases
{
    [Authorize(Policy = "IsRegisteredForEvent")]
    public abstract class EventSpecificPageModel : PageModel
    {
        [FromRoute]
        [ModelBinder(typeof(EventBinder))]
        public Event Event { get; set; }

        [FromRoute]
        [ModelBinder(typeof(RoleBinder))]
        public EventRole EventRole { get; set; }

        private PuzzleUser loggedInUser;

        public PuzzleUser LoggedInUser
        {
            get
            {
                if (loggedInUser == null)
                {
                    loggedInUser = PuzzleUser.GetPuzzleUserForCurrentUser(_context, User, userManager).Result;
                }
                return loggedInUser;
            }
        }

        protected readonly PuzzleServerContext _context;
        private readonly UserManager<IdentityUser> userManager;

        public EventSpecificPageModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager)
        {
            _context = serverContext;
            userManager = manager;
        }

        public async Task<bool> IsRegisteredUser()
        {
            if (LoggedInUser == null)
            {
                return false;
            }
            return await LoggedInUser.IsPlayerInEvent(_context, Event);
        }

        public async Task<bool> IsEventAuthor()
        {
            if (LoggedInUser == null)
            {
                return false;
            }
            return await LoggedInUser.IsAuthorForEvent(_context, Event);
        }

        public async Task<bool> IsEventAdmin()
        {
            if (LoggedInUser == null)
            {
                return false;
            }
            return await LoggedInUser.IsAdminForEvent(_context, Event);
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
