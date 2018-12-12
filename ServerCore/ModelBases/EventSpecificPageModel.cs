using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;

namespace ServerCore.ModelBases
{
    public abstract class EventSpecificPageModel : PageModel
    {
        [FromRoute]
        [ModelBinder(typeof(EventBinder))]
        public Event Event { get; set; }

        private PuzzleUser loggedInUser;

        public PuzzleUser LoggedInUser
        {
            get
            {
                if (loggedInUser == null)
                {
                    loggedInUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleServerContext, User, userManager);
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
    }
}
