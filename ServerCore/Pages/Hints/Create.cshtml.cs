using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class CreateModel : EventSpecificPageModel
    {
        public Puzzle Puzzle { get; set; }

        public CreateModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            return Page();
        }

        [BindProperty]
        public Hint Hint { get; set; }

        public async Task<IActionResult> OnPostAsync(int puzzleID)
        {
            Puzzle puzzle = (from puzz in _context.Puzzles
                             where puzz.ID == puzzleID
                             select puzz).SingleOrDefault();
            if (puzzle == null)
            {
                return NotFound();
            }

            Hint.Puzzle = puzzle;
            ModelState.Remove("Hint.Puzzle");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                _context.Hints.Add(Hint);

                if (puzzle.IsForSinglePlayer)
                {
                    var players = from player in _context.PlayerInEvent
                                where player.Event == Event
                                select player;

                    foreach (PlayerInEvent player in players)
                    {
                        _context.SinglePlayerPuzzleHintStatePerPlayer.Add(new SinglePlayerPuzzleHintStatePerPlayer() 
                        {
                            Hint = Hint,
                            PlayerID = player.PlayerId
                        });
                    }
                }
                else
                {
                    var teams = from team in _context.Teams
                                where team.Event == Event
                                select team;

                    foreach (Team team in teams)
                    {
                        _context.HintStatePerTeam.Add(new HintStatePerTeam() { Hint = Hint, Team = team });
                    }
                }

                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            return RedirectToPage("./Index");
        }
    }
}