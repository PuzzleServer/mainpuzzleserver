using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    /// <summary>
    /// Model for author/admin's team-centric Status page.
    /// used for tracking what each team's progress is and altering that progress manually if needed.
    /// An author's view should be filtered to puzzles where they are an author (NYI so far though).
    /// </summary>
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class StatusModel : PuzzleStatePerTeamPageModel
    {
        public StatusModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public Team Team { get; set; }

        protected override SortOrder DefaultSort => SortOrder.PuzzleAscending;

        public async Task<IActionResult> OnGetAsync(int teamId, SortOrder? sort)
        {
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);

            if (Team == null)
            {
                return NotFound();
            }

            return await InitializeModelAsync(null, Team, sort: sort);
        }

        public async Task<IActionResult> OnGetUnlockStateAsync(int teamId, int? puzzleId, bool value, string sort)
        {
            var puzzle = puzzleId == null ? null : await _context.Puzzles.FirstAsync(m => m.ID == puzzleId.Value);
            var team = await _context.Teams.FirstAsync(m => m.ID == teamId);

            if (team == null)
            {
                return NotFound();
            }

            await SetUnlockStateAsync(puzzle, team, value);

            // redirect without the unlock info to keep the URL clean
            return RedirectToPage(new { teamId, sort });
        }

        public async Task<IActionResult> OnGetSolveStateAsync(int teamId, int? puzzleId, bool value, string sort)
        {
            var puzzle = puzzleId == null ? null : await _context.Puzzles.FirstAsync(m => m.ID == puzzleId.Value);
            var team = await _context.Teams.FirstAsync(m => m.ID == teamId);

            if (team == null)
            {
                return NotFound();
            }

            await SetSolveStateAsync(puzzle, team, value);

            // redirect without the solve info to keep the URL clean
            return RedirectToPage(new { teamId, sort });
        }

        public async Task<IActionResult> OnGetEmailModeAsync(int teamId, int? puzzleId, bool value, string sort)
        {
            var puzzle = puzzleId == null ? null : await _context.Puzzles.FirstAsync(m => m.ID == puzzleId);
            var team = await _context.Teams.FirstAsync(m => m.ID == teamId);

            if (team == null)
            {
                return NotFound();
            }

            await SetEmailModeAsync(puzzle, team, value);

            // redirect without the solve info to keep the URL clean
            return RedirectToPage(new { teamId, sort });
        }
    }
}
