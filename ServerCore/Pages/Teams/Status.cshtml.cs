using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    /// <summary>
    /// Model for author/admin's team-centric Status page.
    /// /used for tracking what each team's progress is and altering that progress manually if needed.
    /// An author's view should be filtered to puzzles where they are an author (NYI so far though).
    /// </summary>
    public class StatusModel : PuzzleStatePerTeamPageModel
    {
        public StatusModel(ServerCore.Models.PuzzleServerContext context) : base(context)
        {
        }

        public Team Team { get; set; }

        protected override SortOrder DefaultSort => SortOrder.PuzzleAscending;

        public async Task<IActionResult> OnGetAsync(int id, SortOrder? sort)
        {
            Team = await Context.Teams.FirstOrDefaultAsync(m => m.ID == id);

            if (Team == null)
            {
                return NotFound();
            }

            await InitializeModelAsync(puzzleId: null, teamId: id, sort: sort);
            return Page();
        }

        public async Task<IActionResult> OnGetUnlockStateAsync(int id, int? puzzleId, bool value, string sort)
        {
            await SetUnlockStateAsync(puzzleId: puzzleId, teamId: id, value: value);

            // redirect without the unlock info to keep the URL clean
            return RedirectToPage(new { id, sort });
        }

        public async Task<IActionResult> OnGetSolveStateAsync(int id, int? puzzleId, bool value, string sort)
        {
            await SetSolveStateAsync(puzzleId: puzzleId, teamId: id, value: value);

            // redirect without the solve info to keep the URL clean
            return RedirectToPage(new { id, sort });
        }
    }
}
