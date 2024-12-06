using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ServerCore.DataModel;
using ServerCore.ModelBases;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsEventAdmin")]
    public class EditModel : EventSpecificPageModel
    {
        private IHubContext<ServerMessageHub> messageHub;

        public EditModel(PuzzleServerContext context, UserManager<IdentityUser> userManager, IHubContext<ServerMessageHub> messageHub) : base(context, userManager)
        {
            this.messageHub = messageHub;
        }

        // Admittedly unusual pattern, with the noble purpose of putting the [BindProperty] attribute on here without leaving it permanently on for every page.
        // Note that naming this property "Event" and using the "new" keyword was sadly insufficient.
        [BindProperty]
        public Event EditableEvent { get; set; }

        public IActionResult OnGet(int id)
        {
            EditableEvent = Event;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!string.IsNullOrEmpty(EditableEvent.Announcement) && EditableEvent.Announcement != Event.Announcement)
            {
                await this.messageHub.SendNotification(Event, "Announcement", EditableEvent.Announcement, isCritical: true);
            }

            // not using attach here because Event is already attached
            _context.Entry(Event).State = EntityState.Detached;
            _context.Attach(EditableEvent).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Details");
        }
    }
}
