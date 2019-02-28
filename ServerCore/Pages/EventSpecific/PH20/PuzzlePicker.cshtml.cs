using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.EventSpecific.PH20
{
    [Authorize(Policy = "PlayerCanSeePuzzle")]
    public class PuzzlePickerModel : EventSpecificPageModel
    {
        public PuzzlePickerModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public async Task<IActionResult> OnGet(int puzzleId)
        {
            Puzzle puzzle = await (from puzz in _context.Puzzles
                                   where puzz.ID == puzzleId
                                   select puzz).FirstOrDefaultAsync();
            if (puzzle == null)
            {
                return NotFound();
            }

            Team team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            var puzzleStateQuery = PuzzleStateHelper.GetFullReadOnlyQuery(_context, Event, puzzle, team);
            PuzzleStatePerTeam state = await (from pspt in puzzleStateQuery
                                        where pspt.PuzzleID == puzzle.ID && pspt.TeamID == team.ID
                                        select pspt).FirstOrDefaultAsync();
            if (state == null)
            {
                return NotFound();
            }

            // Only move forward if the puzzle is open and unsolved
            if (state.UnlockedTime == null || state.SolvedTime != null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}