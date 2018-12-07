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

        public PuzzleUser LoggedInUser { get; set; }

        private PuzzleServerContext puzzleServerContext;

        public EventSpecificPageModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager)
        {
            puzzleServerContext = serverContext;

            // Get the currently signed in PuzzleUser
            LoggedInUser = PuzzleUser.GetPuzzleUserForCurrentUser(puzzleServerContext, User, userManager);
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
