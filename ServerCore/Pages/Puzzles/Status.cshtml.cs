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
    /// Model for author/admin's puzzle-centric Status page.
    /// used for tracking what each team's progress is and altering that progress manually if needed.
    /// </summary>
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class StatusModel : PuzzleStatePerTeamPageModel
    {
        public StatusModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public Puzzle Puzzle { get; set; }

        protected override SortOrder DefaultSort => SortOrder.TeamAscending;

        public async Task<IActionResult> OnGetAsync(int puzzleId, SortOrder? sort)
        {
            Puzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.ID == puzzleId);

            if (Puzzle == null)
            {
                return NotFound();
            }

            return await InitializeModelAsync(Puzzle, null, sort: sort);
        }

        public async Task<IActionResult> OnGetUnlockStateAsync(int puzzleId, int? teamId, bool value, string sort)
        {
            var puzzle = await _context.Puzzles.FirstAsync(m => m.ID == puzzleId);
            var team = teamId == null ? null : await _context.Teams.FirstAsync(m => m.ID == teamId.Value);

            if (puzzle == null)
            {
                return NotFound();
            }

            await SetUnlockStateAsync(puzzle, team, value);

            // redirect without the unlock info to keep the URL clean
            return RedirectToPage(new { puzzleId, sort });
        }

        public async Task<IActionResult> OnGetSolveStateAsync(int puzzleId, int? teamId, bool value, string sort)
        {
            var puzzle = await _context.Puzzles.FirstAsync(m => m.ID == puzzleId);
            var team = teamId == null ? null : await _context.Teams.FirstAsync(m => m.ID == teamId.Value);

            if (puzzle == null)
            {
                return NotFound();
            }

            await SetSolveStateAsync(puzzle, team, value);

            // redirect without the solve info to keep the URL clean
            return RedirectToPage(new { puzzleId, sort });
        }
    }
}
