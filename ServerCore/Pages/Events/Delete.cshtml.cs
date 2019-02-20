using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsEventAdmin")]
    public class DeleteModel : EventSpecificPageModel
    {
        public DeleteModel(PuzzleServerContext context, UserManager<IdentityUser> userManager) : base(context, userManager)
        {
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Event != null)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    var eventTeams = from Team team in _context.Teams
                                     where team.Event == Event
                                     select team;

                    foreach (Team team in eventTeams)
                    {
                        await TeamHelper.DeleteTeamAsync(_context, team);
                    }

                    var eventPuzzles = from Puzzle puzzle in _context.Puzzles
                                       where puzzle.Event == Event
                                       select puzzle;
                    foreach (Puzzle puzzle in eventPuzzles)
                    {
                        await PuzzleHelper.DeletePuzzleAsync(_context, puzzle);
                    }

                    _context.Events.Remove(Event);

                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
