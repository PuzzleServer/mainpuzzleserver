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
                    loggedInUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleServerContext, User, userManager).Result;
                }

                return loggedInUser;
            }
        }

        private PuzzleServerContext puzzleServerContext;
        private UserManager<IdentityUser> userManager;

        public EventSpecificPageModel()
        {
            // Default constructor - note that pages that use this constructor won't know what PuzzleUser is signed in
        }

        public EventSpecificPageModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager)
        {
            puzzleServerContext = serverContext;
            userManager = manager;
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
