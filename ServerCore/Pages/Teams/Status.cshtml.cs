using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class StatusModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public StatusModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public Team Team { get; set; }

        public IList<PuzzleStatePerTeam> PuzzleStatePerTeam { get;set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == id);

            if (Team == null)
            {
                return NotFound();
            }

            PuzzleStatePerTeam = await _context.PuzzleStatePerTeam.Where(state => state.Team == Team).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetUnlockStateAsync(int id, int? puzzleId, bool value)
        {
            var states = await this.GetStates(id, puzzleId);

            for (int i = 0; i < states.Count; i++)
            {
                states[i].IsUnlocked = value;
            }
            await _context.SaveChangesAsync();

            // redirect without the unlock info to keep the URL clean
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnGetSolveStateAsync(int id, int? puzzleId, bool value)
        {
            var states = await this.GetStates(id, puzzleId);

            for (int i = 0; i < states.Count; i++)
            {
                states[i].IsSolved = value;
            }
            await _context.SaveChangesAsync();

            // redirect without the solve info to keep the URL clean
            return RedirectToPage(new { id });
        }

        private Task<List<PuzzleStatePerTeam>> GetStates(int teamId, int? puzzleId)
        {
            var stateQ = _context.PuzzleStatePerTeam.Where(s => s.Team.ID == teamId);

            if (puzzleId.HasValue)
            {
                stateQ = stateQ.Where(s => s.Puzzle.ID == puzzleId.Value);
            }

            return stateQ.ToListAsync();
        }
    }
}
