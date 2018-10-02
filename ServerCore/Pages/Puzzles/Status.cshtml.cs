using System.Threading.Tasks;
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
    public class StatusModel : PuzzleStatePerTeamPageModel
    {
        public StatusModel(PuzzleServerContext context) : base(context)
        {
        }

        public Puzzle Puzzle { get; set; }

        protected override SortOrder DefaultSort => SortOrder.TeamAscending;

        public async Task<IActionResult> OnGetAsync(int id, SortOrder? sort)
        {
            Puzzle = await Context.Puzzles.FirstOrDefaultAsync(m => m.ID == id);

            if (Puzzle == null)
            {
                return NotFound();
            }

            await InitializeModelAsync(Puzzle, null, sort: sort);
            return Page();
        }

        public async Task<IActionResult> OnGetUnlockStateAsync(int id, int? teamId, bool value, string sort)
        {
            var puzzle = await Context.Puzzles.FirstAsync(m => m.ID == id);
            var team = teamId == null ? null : await Context.Teams.FirstAsync(m => m.ID == teamId.Value);

            await SetUnlockStateAsync(puzzle, team, value);

            // redirect without the unlock info to keep the URL clean
            return RedirectToPage(new { id, sort });
        }

        public async Task<IActionResult> OnGetSolveStateAsync(int id, int? teamId, bool value, string sort)
        {
            var puzzle = await Context.Puzzles.FirstAsync(m => m.ID == id);
            var team = teamId == null ? null : await Context.Teams.FirstAsync(m => m.ID == teamId.Value);

            await SetSolveStateAsync(puzzle, team, value);

            // redirect without the solve info to keep the URL clean
            return RedirectToPage(new { id, sort });
        }
    }
}
