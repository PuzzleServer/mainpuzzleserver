using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Pages
{
    public class CustomHtmlPageLoader : Controller
    {
        private readonly PuzzleServerContext context;
        private readonly UserManager<IdentityUser> userManager;

        public CustomHtmlPageLoader(PuzzleServerContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        // GET api/custom
        [HttpGet]
        [Route("{eventId}/{puzzleId}/api/custom/")]
        public async Task<IActionResult> Index(string eventId, int puzzleId)
        {
            Event currentEvent = await EventHelper.GetEventFromEventId(context, eventId);
            if (currentEvent == null)
            {
                return Content("ERROR:  That event doesn't exist");
            }

            PuzzleUser user = await PuzzleUser.GetPuzzleUserForCurrentUser(context, User, userManager);
            if (user == null)
            {
                return Content("ERROR:  You aren't logged in");
            }

            Team team = await UserEventHelper.GetTeamForPlayer(context, currentEvent, user);
            if (team == null)
            {
                return Content("ERROR:  You're not on a team");
            }

            Puzzle thisPuzzle = await context.Puzzles.FirstOrDefaultAsync(m => m.ID == puzzleId);
            if (thisPuzzle == null)
            {
                return Content("ERROR:  That's not a valid puzzle ID");
            }

            if (!currentEvent.AreAnswersAvailableNow)
            {
                var puzzleState = await (from state in context.PuzzleStatePerTeam
                                         where state.Puzzle == thisPuzzle && state.Team == team
                                         select state).FirstOrDefaultAsync();
                if (puzzleState == null || puzzleState.UnlockedTime == null)
                {
                    return Content("ERROR:  You haven't unlocked this puzzle yet");
                }
            }

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

        // POST api/custom/submit
        [HttpPost]
        [Route("{eventId}/{puzzleId}/api/custom/submit/")]
        public async Task<IActionResult> Post(string eventId, int puzzleId, string submissionText)
        {
            // Find what team this user is on, relative to the event.

            Event currentEvent = await EventHelper.GetEventFromEventId(context, eventId);
            if (currentEvent == null) { return Unauthorized(); }
            PuzzleUser user = await PuzzleUser.GetPuzzleUserForCurrentUser(context, User, userManager);
            if (user == null) { return Unauthorized(); }
            Team team = await UserEventHelper.GetTeamForPlayer(context, currentEvent, user);
            if (team == null) { return Unauthorized(); }

            var response = SubmissionEvaluator.EvaluateSubmission(context, user, puzzleId, submissionText);
            return Json(response);
        }
    }
}
