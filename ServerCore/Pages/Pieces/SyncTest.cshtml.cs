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

namespace ServerCore.Pages.Pieces
{
    [AllowAnonymous]
    public class SyncTestModel : EventSpecificPageModel
    {
        public int EventId { get; set; }
        public int KitchenSyncId { get; set; }
        public int HeatSyncId { get; set; }
        public int LipSyncId { get; set; }
        public int MetapuzzleId { get; set; }
        public string InitialSyncString { get; set; }

        public SyncTestModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager): base(serverContext, userManager)
        {
        }

        [Authorize(Policy = "IsEventAdmin")]
        public async Task<IActionResult> OnGetAsync(string eventId, int puzzleId)
        {
            Event currentEvent = await EventHelper.GetEventFromEventId(_context, eventId);
            if (currentEvent == null) { return Unauthorized(); }
            EventId = currentEvent.ID;

            PuzzleUser user = LoggedInUser;
            if (user == null) { return Unauthorized(); }

            Team team = await UserEventHelper.GetTeamForPlayer(_context, currentEvent, user);
            if (team == null) { return Unauthorized(); }

            Puzzle kitchenSyncPuzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.Name == "Kitchen Sync" && m.Event.ID == EventId);
            KitchenSyncId = kitchenSyncPuzzle.ID;
            Puzzle heatSyncPuzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.Name == "Heat Sync" && m.Event.ID == EventId);
            HeatSyncId = heatSyncPuzzle.ID;
            Puzzle lipSyncPuzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.Name == "Lip Sync" && m.Event.ID == EventId);
            LipSyncId = lipSyncPuzzle.ID;
            Puzzle syncTestMetapuzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.Name == "Sync Test" && m.Event.ID == EventId);
            MetapuzzleId = syncTestMetapuzzle.ID;

            var helper = new SyncHelper(_context);
            List<int> query_puzzle_ids = new List<int> { KitchenSyncId, HeatSyncId, LipSyncId };
            var response = await helper.GetSyncResponse(currentEvent.ID, team.ID, puzzleId, query_puzzle_ids, 0, null, null);
            var responseSerialized = JsonConvert.SerializeObject(response);
            InitialSyncString = HttpUtility.JavaScriptStringEncode(responseSerialized);

            return Page();
        }
    }
}
