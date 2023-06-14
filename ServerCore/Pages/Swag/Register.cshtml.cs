using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Swag
{
    [Authorize(Policy = "IsRegisteredForEvent")]
    public class SwagRegisterModel : EventSpecificPageModel
    {
        public SwagRegisterModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public PlayerInEvent PlayerInEvent { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!Event.EventHasSwag)
            {
                return Forbid("This page is only available for events that have swag.");
            }

            PlayerInEvent = await _context.PlayerInEvent.Where(m => m.Event == Event && m.Player == LoggedInUser).FirstOrDefaultAsync();

            if (PlayerInEvent == null)
            {
                PlayerInEvent = new PlayerInEvent();
            }

            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            ModelState.Remove("Swag.Event");
            ModelState.Remove("Swag.Player");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            PlayerInEvent editableSwag = await _context.PlayerInEvent.Where(m => m.Event == Event && m.Player == LoggedInUser).FirstOrDefaultAsync();

            if (editableSwag == null)
            {
                PlayerInEvent.Player = LoggedInUser;
                PlayerInEvent.Event = Event;
                _context.PlayerInEvent.Add(PlayerInEvent);
            }
            else
            {
                editableSwag.Lunch = PlayerInEvent.Lunch;
                editableSwag.LunchModifications = PlayerInEvent.LunchModifications;
                editableSwag.Player = LoggedInUser;
                editableSwag.Event = Event;

                _context.Attach(editableSwag).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/EventSpecific/Index");
        }
    }
}