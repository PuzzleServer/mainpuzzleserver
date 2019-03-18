using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.EventSpecific.PH20
{
    public class ThePlayThatGoesWrongModel : EventSpecificPageModel
    {
        public int EventId { get; set; }
        public int MetapuzzleId { get; set; }
        public string InitialSyncString { get; set; }

        public ThePlayThatGoesWrongModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager): base(serverContext, userManager)
        {
        }

        [Authorize(Policy = "PlayerCanSeePuzzle")]
        public async Task<IActionResult> OnGetAsync(string eventId, int puzzleId)
        {
            Event currentEvent = await EventHelper.GetEventFromEventId(_context, eventId);
            if (currentEvent == null) { return Unauthorized(); }
            EventId = currentEvent.ID;

            PuzzleUser user = LoggedInUser;
            if (user == null) { return Unauthorized(); }

            Team team = await UserEventHelper.GetTeamForPlayer(_context, currentEvent, user);
            if (team == null) { return Unauthorized(); }

            Puzzle thisPuzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.ID == puzzleId && m.Event.ID == EventId);
            if (thisPuzzle == null || !thisPuzzle.Name.Equals("The Play That Goes Wrong")) {
                return Unauthorized();
            }
            MetapuzzleId = puzzleId;

            var helper = new SyncHelper(_context);
            List<int> query_puzzle_ids = Enumerable.Range(MetapuzzleId - 10, 11).ToList();
            var response = await helper.GetSyncResponse(currentEvent.ID, team.ID, puzzleId, query_puzzle_ids, 0, null, null);
            var responseSerialized = JsonConvert.SerializeObject(response);
            InitialSyncString = HttpUtility.JavaScriptStringEncode(responseSerialized);

            return Page();
        }
    }
}
