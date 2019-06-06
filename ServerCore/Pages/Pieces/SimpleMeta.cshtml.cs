using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Pieces
{
    [Authorize(Policy = "PlayerIsOnTeam")]
    public class SimpleMetaModel : EventSpecificPageModel
    {
        public SimpleMetaModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Piece> EarnedPieces { get; set; }

        public Puzzle Puzzle { get; set; }
        
        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            Puzzle = await _context.Puzzles.Where(p => p.ID == puzzleId).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound("Puzzle does not exist.");
            }

            Team team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            int solvedPuzzleCount = 0;

            switch(Puzzle.PieceMetaUsage)
            {
                case PieceMetaUsage.EntireEvent:
                    solvedPuzzleCount = await _context.PuzzleStatePerTeam.Where(ps => ps.Team == team && ps.SolvedTime != null && ps.Puzzle.SolveValue >= 10).CountAsync();
                    break;

                case PieceMetaUsage.GroupOnly:
                    solvedPuzzleCount = await _context.PuzzleStatePerTeam.Where(ps => ps.Team == team && ps.SolvedTime != null && ps.Puzzle.SolveValue >= 10 && ps.Puzzle.Group == Puzzle.Group).CountAsync();
                    break;

                default:
                    return NotFound("Puzzle does not support the simple meta view.");
            }

            EarnedPieces = await _context.Pieces.Where(p => p.PuzzleID == puzzleId && p.ProgressLevel <= solvedPuzzleCount).OrderBy(p => p.ProgressLevel).ToListAsync();

            return Page();
        }
    }
}
