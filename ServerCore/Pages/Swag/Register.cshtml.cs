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
        public ServerCore.DataModel.Swag Swag { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!Event.EventHasSwag)
            {
                return Forbid("This page is only available for events that have swag.");
            }

            Swag = await _context.Swag.Where(m => m.Event == Event && m.Player == LoggedInUser).FirstOrDefaultAsync();

            if (Swag == null)
            {
                Swag = new ServerCore.DataModel.Swag();
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

            ServerCore.DataModel.Swag editableSwag = await _context.Swag.Where(m => m.Event == Event && m.Player == LoggedInUser).FirstOrDefaultAsync();

            if (editableSwag == null)
            {
                Swag.Player = LoggedInUser;
                Swag.Event = Event;
                _context.Swag.Add(Swag);
            }
            else
            {
                editableSwag.Lunch = Swag.Lunch;
                editableSwag.LunchModifications = Swag.LunchModifications;
                editableSwag.Player = LoggedInUser;
                editableSwag.Event = Event;

                _context.Attach(editableSwag).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/EventSpecific/Index");
        }
    }
}