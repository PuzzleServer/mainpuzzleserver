using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    /// <summary>
    /// Model for author/admin's puzzle-centric Status page for puzzles that can be solved by single players as opposed to being solved on a team.
    /// used for tracking what each team's progress is and altering that progress manually if needed.
    /// </summary>
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class SinglePlayerPuzzleStatusModel : SinglePlayerPuzzleStatePerPlayerPageModel
    {
        public SinglePlayerPuzzleStatusModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public DateTime? UnlockedTime { get; set; }

        public Puzzle Puzzle { get; set; }

        protected override SortOrder DefaultSort => SortOrder.UserAscending;

        public async Task<IActionResult> OnGetAsync(int puzzleId, SortOrder? sort)
        {
            SinglePlayerPuzzleUnlockState unlockState = await _context.SinglePlayerPuzzleUnlockStates.FirstOrDefaultAsync(m => m.PuzzleID == puzzleId);

            if (unlockState == null)
            {
                return NotFound();
            }

            Puzzle = unlockState.Puzzle;
            UnlockedTime = unlockState.UnlockedTime;
            return await InitializeModelAsync(unlockState, sort: sort);
        }

        public async Task<IActionResult> OnGetUnlockStateAsync(int puzzleId, int? playerId, bool value, string sort)
        {
            await SinglePlayerPuzzleStateHelper.SetUnlockStateAsync(
                _context,
                Event,
                puzzleId,
                playerId,
                value ? DateTime.UtcNow : null);

            // redirect without the unlock info to keep the URL clean
            return RedirectToPage(new { puzzleId, sort });
        }

        public async Task<IActionResult> OnGetSolveStateAsync(int puzzleId, int? playerId, bool value, string sort)
        {
            await SetSolveStateAsync(puzzleId, playerId, value);

            // redirect without the solve info to keep the URL clean
            return RedirectToPage(new { puzzleId, sort });
        }

        public async Task<IActionResult> OnGetEmailModeAsync(int puzzleId, int? playerId, bool value, string sort)
        {
            var puzzle = await _context.Puzzles.FirstAsync(m => m.ID == puzzleId);

            if (puzzle == null)
            {
                return NotFound();
            }

            await SetEmailModeAsync(puzzle, playerId, value);

            // redirect without the solve info to keep the URL clean
            return RedirectToPage(new { puzzleId, sort });
        }
    }
}