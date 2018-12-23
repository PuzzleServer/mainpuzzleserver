using System;
using System.Linq;
using System.Threading.Tasks;
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
                    loggedInUser = PuzzleUser.GetPuzzleUserForCurrentUser(_context, User, userManager);
                }

                return loggedInUser;
            }
        }

        protected readonly PuzzleServerContext _context;
        private UserManager<IdentityUser> userManager;

        public EventSpecificPageModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager)
        {
            _context = serverContext;
            userManager = manager;
        }

        public async Task<bool> CanAdminPuzzle(Puzzle puzzle)
        {
            if (EventRole != EventRole.admin && EventRole != EventRole.author)
            {
                return false;
            }

            if (puzzle == null || (EventRole == EventRole.author && !await UserEventHelper.IsAuthorOfPuzzle(_context, puzzle, LoggedInUser)))
            {
                return false;
            }

            return true;
        }

        public class EventBinder : IModelBinder
        {
            public async Task BindModelAsync(ModelBindingContext bindingContext)
            {
                string eventIdAsString = bindingContext.ActionContext.RouteData.Values["eventId"] as string;

                if (int.TryParse(eventIdAsString, out int eventId))
                {
                    var puzzleServerContext = bindingContext.HttpContext.RequestServices.GetService<PuzzleServerContext>();
                    Event eventObj = await puzzleServerContext.Events.Where(e => e.ID == eventId).FirstOrDefaultAsync();

                    if (eventObj != null)
                    {
                        bindingContext.Result = ModelBindingResult.Success(eventObj);
                    }
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
