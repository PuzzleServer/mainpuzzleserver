using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class StatusModel : PuzzleStatePerTeamPageModel
    {
        public StatusModel(ServerCore.Models.PuzzleServerContext context) : base(context)
        {
        }

        public Team Team { get; set; }

        protected override string DefaultSort => "puzzle";

        public async Task<IActionResult> OnGetAsync(int id, string sort)
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
