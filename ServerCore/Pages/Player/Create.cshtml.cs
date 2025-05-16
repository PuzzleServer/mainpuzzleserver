using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
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


            // Immediately create the PlayerInEvent and redirect if the event doesn't require specific information from the player
            if (!EventHelper.EventRequiresActivePlayerRegistration(Event))
            {
                player = new PlayerInEvent
                {
                    EventId = Event.ID,
                    Event = Event,
                    PlayerId = LoggedInUser.ID,
                    Player = LoggedInUser
                };

                _context.PlayerInEvent.Add(player);
                await _context.SaveChangesAsync();
            }

            if (player != null)
            {
                return RedirectToPage("/Teams/Signup");
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

            return RedirectToPage("/Teams/Signup");
        }
    }
}
