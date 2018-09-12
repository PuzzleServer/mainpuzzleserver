using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class StatusModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public StatusModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public Puzzle Puzzle { get; set; }

        public IList<PuzzleStatePerTeam> PuzzleStatePerTeam { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Puzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.ID == id);

            if (Puzzle == null)
            {
                return NotFound();
            }

            await EnsurePuzzleStateForPuzzleAsync();

            PuzzleStatePerTeam = await _context.PuzzleStatePerTeam.Where(state => state.Puzzle == Puzzle).ToListAsync();
            return Page();
        }

        // TODO: Not entirely sure this should be a Get but I can't figure out how to have an anchor tag do a post.
        public async Task<IActionResult> OnGetUnlockStateAsync(int id, int? teamId, bool value)
        {
            var states = await this.GetStates(id, teamId);

            for (int i = 0; i < states.Count; i++)
            {
                states[i].IsUnlocked = value;
            }
            await _context.SaveChangesAsync();

            // TODO: Is there a cleaner way to do this part?
            return await OnGetAsync(id);
        }

        // TODO: Not entirely sure this should be a Get but I can't figure out how to have an anchor tag do a post.
        public async Task<IActionResult> OnGetSolveStateAsync(int id, int? teamId, bool value)
        {
            var states = await this.GetStates(id, teamId);

            for (int i = 0; i < states.Count; i++)
            {
                states[i].IsSolved = value;
            }
            await _context.SaveChangesAsync();

            // TODO: Is there a cleaner way to do this part?
            return await OnGetAsync(id);
        }

        private Task<List<PuzzleStatePerTeam>> GetStates(int puzzleId, int? teamId)
        {
            var stateQ = _context.PuzzleStatePerTeam.Where(s => s.Puzzle.ID == puzzleId);

            if (teamId.HasValue)
            {
                stateQ = stateQ.Where(s => s.Puzzle.ID == teamId.Value);
            }

            return stateQ.ToListAsync();
        }

        private async Task EnsurePuzzleStateForPuzzleAsync()
        {
            // TODO: Surely there is some magic join here that works, but despite reading and rereading, I do not understand how join syntax works. At all.
            // I am looking for all Puzzles in this event for which this team has no PuzzleStatePerTeam.
            //
            // If there is an efficient way to validate full PuzzleStatePerTeam integrity across all puzzles/teams in an event,
            // we could run that at app start, puzzle add/delete, and team add/delete.
            var teamsQ = _context.Teams.Where(team => team.Event == Event);
            var puzzleStateTeamsQ = _context.PuzzleStatePerTeam.Where(state => state.Puzzle == Puzzle).Select(state => state.Team);
            var teamsWithoutState = await teamsQ.Except(puzzleStateTeamsQ).ToListAsync();

            if (teamsWithoutState.Count > 0)
            {
                for (int i = 0; i < teamsWithoutState.Count; i++)
                {
                    _context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { Puzzle = Puzzle, Team = teamsWithoutState[i] });
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
