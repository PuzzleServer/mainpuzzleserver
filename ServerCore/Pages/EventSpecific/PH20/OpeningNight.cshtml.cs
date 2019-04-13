using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.EventSpecific.PH20
{
    class BackstageSolveStatus
    {
        public string PuzzleName { get; set; }
        public int SolveTime { get; set; }
    }

    [Authorize(Policy = "PlayerCanSeePuzzle")]
    public class OpeningNightModel : EventSpecificPageModel
    {
        public OpeningNightModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public async Task<IActionResult> OnGet(int puzzleId)
        {
            Puzzle puzzle = await(from puzz in _context.Puzzles
                                  where puzz.ID == puzzleId
                                  select puzz).FirstOrDefaultAsync();
            if (puzzle == null)
            {
                return NotFound();
            }

            // Restrict to the Backstage metapuzzle
            if (puzzle.Group != "Backstage")
            {
                return NotFound();
            }
            if (puzzle.SolveValue != 50)
            {
                return NotFound();
            }

            Team team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);

            var puzzleQuery = PuzzleStateHelper.GetFullReadOnlyQuery(_context, Event, null, team, null);
            var solvedBackstagePuzzles = await (from PuzzleStatePerTeam pspt in puzzleQuery
                                                where pspt.SolvedTime != null && pspt.UnlockedTime != null && pspt.Puzzle.IsPuzzle && pspt.Puzzle.Group == "Backstage"
                                                select new BackstageSolveStatus() { PuzzleName = pspt.Puzzle.Name, SolveTime = (int)(pspt.SolvedTime.Value - pspt.UnlockedTime.Value).TotalSeconds }).ToListAsync();

            string json = JsonConvert.SerializeObject(solvedBackstagePuzzles);
            string escapedJson = Uri.EscapeDataString(json);
            byte[] macKey = Encoding.ASCII.GetBytes(puzzle.Description);
            HMACSHA1 hasher = new HMACSHA1(macKey);
            byte[] jsonBytes = Encoding.ASCII.GetBytes(escapedJson);
            byte[] mac = hasher.ComputeHash(jsonBytes);
            string macString = Convert.ToBase64String(mac).Replace('/', '_').Replace('+', '.');

            return Redirect($"https://ph20backstage.azurewebsites.net/openingnight/{macString}/{escapedJson}");
        }
    }
}