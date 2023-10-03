using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Data.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Player
{
    // An empty Authorize attribute requires that the person is signed in but sets no other requirements
    [Authorize]
    public class CreateModel : EventSpecificPageModel
    {

        public CreateModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGet()
        {
            // If the player already has a profile for this event but ended up here we need to kick them over to details instead
            PlayerInEvent player = await (from p in _context.PlayerInEvent
                                          where p.Player == LoggedInUser &&
                                          p.Event == Event
                                          select p).FirstOrDefaultAsync();

            if(player != null)
            {
                return RedirectToPage("/Teams/List");
            }

            return Page();
        }

        [BindProperty]
        public PlayerInEvent PlayerInEvent { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("EventId");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            PlayerInEvent.EventId = Event.ID;
            PlayerInEvent.Event = Event;
            PlayerInEvent.PlayerId = LoggedInUser.ID;
            PlayerInEvent.Player = LoggedInUser;

            _context.PlayerInEvent.Add(PlayerInEvent);
            await _context.SaveChangesAsync();

            List<Hint> singlePlayerPuzzleHints = await (from Hint hint in _context.Hints
                               where hint.Puzzle.EventID == Event.ID && hint.Puzzle.IsForSinglePlayer
                               select hint).ToListAsync();

            foreach (Hint hint in singlePlayerPuzzleHints)
            {
                _context.SinglePlayerPuzzleHintStatePerPlayer.Add(new SinglePlayerPuzzleHintStatePerPlayer()
                {
                    Hint = hint,
                    PlayerID = LoggedInUser.ID
                });
            }
            await _context.SaveChangesAsync();

            return RedirectToPage("/Teams/List");
        }
    }
}
