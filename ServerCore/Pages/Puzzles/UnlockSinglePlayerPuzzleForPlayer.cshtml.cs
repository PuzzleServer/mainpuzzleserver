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
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class UnlockSinglePlayerPuzzleForPlayerModel : EventSpecificPageModel
    {
        public Puzzle Puzzle { get; set; }

        public IList<PuzzleUser> Users { get; set; }

        public UnlockSinglePlayerPuzzleForPlayerModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            Puzzle = (await (from puzzle in _context.Puzzles
                                 where puzzle.ID == puzzleId
                                 select puzzle).ToListAsync()).SingleOrDefault();

            if (Puzzle == null)
            {
                return NotFound();
            }

            HashSet<int> playerIdsAlreadyUnlocked = (await (from puzzleState in _context.SinglePlayerPuzzleStatePerPlayer
                                                 where puzzleState.UnlockedTime.HasValue && puzzleState.PuzzleID == puzzleId
                                                 select puzzleState.PlayerID).ToListAsync()).ToHashSet();

            Users = await (from user in _context.PlayerInEvent
                           where user.EventId == this.Event.ID
                           where !playerIdsAlreadyUnlocked.Contains(user.PlayerId)
                           select user.Player).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetUnlockPlayerAsync(int userId, int puzzleId)
        {
            PuzzleUser user = await _context.PuzzleUsers.FirstOrDefaultAsync(m => m.ID == userId);
            if (user == null)
            {
                return NotFound("Could not find user with ID '" + userId + "'. Check to make sure the user hasn't been removed.");
            }

            var userPuzzleState = (await (from puzzleState in _context.SinglePlayerPuzzleStatePerPlayer
                                         where puzzleState.PlayerID == userId && puzzleState.PuzzleID == puzzleId
                                         select puzzleState)
                                         .ToListAsync()).FirstOrDefault();

            // Check that the user isn't already unlocked
            if (userPuzzleState != null && userPuzzleState.UnlockedTime.HasValue)
            {
                return NotFound("User is already unlocked for this puzzle.");
            }

            // Set user unlock state
            if (userPuzzleState == null)
            {
                _context.SinglePlayerPuzzleStatePerPlayer.Add(
                    new SinglePlayerPuzzleStatePerPlayer
                    { 
                        PuzzleID = puzzleId,
                        PlayerID = userId,
                        UnlockedTime = DateTime.UtcNow
                    });
            }
            else
            {
                userPuzzleState.UnlockedTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Puzzles/SinglePlayerPuzzleStatus", new { puzzleId = puzzleId });
        }
    }
}
