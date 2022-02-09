﻿using System.Collections.Generic;
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

            // TODO: Metas are filtered out by looking at score.
            // Ideally each puzzle would have a flag saying whether it counts or not, 
            // but changing the database is more risk than we need right now.
            IQueryable<PuzzleStatePerTeam> pieceQuery = _context.PuzzleStatePerTeam.Where(ps => ps.Team == team && ps.SolvedTime != null && ps.Puzzle.SolveValue >= 10 && ps.Puzzle.SolveValue < 50);

            if (Puzzle.PieceMetaTagFilter != null)
            {
                pieceQuery = pieceQuery.Where(ps => ps.Puzzle.PieceTag == Puzzle.PieceMetaTagFilter);
            }

            if (Puzzle.PieceMetaUsage == PieceMetaUsage.GroupOnly)
            {
                pieceQuery = pieceQuery.Where(ps => ps.Puzzle.Group == Puzzle.Group);
            }
            else if (Puzzle.PieceMetaUsage != PieceMetaUsage.EntireEvent)
            {
                return NotFound("Puzzle does not support the simple meta view.");
            }

            totalWeight = await pieceQuery.SumAsync(ps => ps.Puzzle.PieceWeight ?? 1);

            EarnedPieces = await _context.Pieces.Where(p => p.PuzzleID == puzzleId && p.ProgressLevel <= totalWeight).OrderBy(p => p.ProgressLevel).ThenBy(p => p.Contents).ToListAsync();

            return Page();
        }
    }
}
