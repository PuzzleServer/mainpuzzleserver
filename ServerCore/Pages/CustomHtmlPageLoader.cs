using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Pages
{
    /// <summary>
    /// Loads an html page from the puzzle's materials
    /// </summary>
    public class CustomHtmlPageLoader : Controller
    {
        private readonly PuzzleServerContext context;
        private readonly UserManager<IdentityUser> userManager;

        public CustomHtmlPageLoader(PuzzleServerContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        // GET {eventId}/{puzzleId}/custom
        [HttpGet]
        [Authorize(Policy = "PlayerCanSeePuzzle")]
        [Route("{eventId}/{puzzleId}/custom/")]
        public async Task<IActionResult> Index(string eventId, int puzzleId)
        {
            Event currentEvent = await EventHelper.GetEventFromEventId(context, eventId);
            PuzzleUser user = await PuzzleUser.GetPuzzleUserForCurrentUser(context, User, userManager);
            Team team = await UserEventHelper.GetTeamForPlayer(context, currentEvent, user);
            Puzzle thisPuzzle = await context.Puzzles.FirstOrDefaultAsync(m => m.ID == puzzleId);

            // Find the material file with the latest-alphabetically ShortName that contains the substring "customPage".
            var materialFile = await (from f in context.ContentFiles
                                      where f.Puzzle == thisPuzzle && f.FileType == ContentFileType.PuzzleMaterial && f.ShortName.Contains("customPage")
                                      orderby f.ShortName descending
                                      select f).FirstOrDefaultAsync();
            if (materialFile == null)
            {
                return Content("ERROR:  There's no custom page uploaded for this puzzle");
            }

            var materialUrl = materialFile.Url;

            // Download that material file.
            string fileContents;
            using (var wc = new System.Net.WebClient())
            {
                fileContents = await wc.DownloadStringTaskAsync(materialUrl);
            }

            // Return the file contents to the user.
            return Content(fileContents, "text/html");
        }
    }
}
