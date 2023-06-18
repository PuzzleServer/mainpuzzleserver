using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    [Authorize(Policy = "PlayerCanSeePuzzle")]
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
            int totalWeight = 0;

            if (string.IsNullOrEmpty(Puzzle.PieceMetaTagFilter))
            {
                // NOTE: By default, puzzles are filtered by looking at score.
                // If you want a more sophisticated filtering mechanism, set a PieceMetaTagFilter (which is a Regex)
                // and set tags on all puzzles you wish to consider.
                IQueryable<PuzzleStatePerTeam> pieceQuery = _context.PuzzleStatePerTeam.Where(ps => ps.Team == team && ps.SolvedTime != null && ps.Puzzle.SolveValue >= 10 && ps.Puzzle.SolveValue < 50);

                if (Puzzle.PieceMetaUsage == PieceMetaUsage.GroupOnly)
                {
                    pieceQuery = pieceQuery.Where(ps => ps.Puzzle.Group == Puzzle.Group);
                }
                else if (Puzzle.PieceMetaUsage != PieceMetaUsage.EntireEvent)
                {
                    return NotFound("Puzzle does not support the simple meta view.");
                }

                totalWeight = await pieceQuery.SumAsync(ps => ps.Puzzle.PieceWeight ?? 1);
            }
            else
            {
                var puzData = await (from pspt in _context.PuzzleStatePerTeam
                                     join puz in _context.Puzzles on pspt.PuzzleID equals puz.ID
                                     where pspt.Team == team &&
                                     pspt.SolvedTime != null &&
                                     (Puzzle.PieceMetaUsage != PieceMetaUsage.GroupOnly || puz.Group == Puzzle.Group)
                                     select new { PieceTag = puz.PieceTag, PieceWeight = puz.PieceWeight }).ToListAsync();

                Regex filterRegex = new Regex(Puzzle.PieceMetaTagFilter);
                foreach (var puz in puzData)
                {
                    if (!string.IsNullOrEmpty(puz.PieceTag) && filterRegex.Match(puz.PieceTag)?.Success == true)
                    {
                        totalWeight += puz.PieceWeight ?? 1;
                    }
                }
            }

            EarnedPieces = await _context.Pieces.Where(p => p.PuzzleID == puzzleId && p.ProgressLevel <= totalWeight).OrderBy(p => p.ProgressLevel).ThenBy(p => p.Contents).ToListAsync();

            return Page();
        }
    }
}
