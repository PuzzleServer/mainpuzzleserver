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
    public class InternMetaModel : EventSpecificPageModel
    {
        public InternMetaModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Piece> EarnedPieces { get;set; }
        
        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            if (!Event.IsInternEvent)
            {
                return Forbid("This page is only valid for intern events.");
            }

            Team team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            int solvedPuzzleCount = await PuzzleStateHelper.GetSparseQuery(_context, Event, null, team, null).Where(ps => ps.SolvedTime != null && ps.Puzzle.SolveValue >= 10).CountAsync();

            EarnedPieces = await _context.Pieces.Where(p => p.PuzzleID == puzzleId && p.ProgressLevel <= solvedPuzzleCount).OrderBy(p => p.ProgressLevel).ToListAsync();

            return Page();
        }
    }
}
