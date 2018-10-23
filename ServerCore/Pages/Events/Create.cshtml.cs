using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCore.DataModel;

namespace ServerCore.Pages.Events
{
    public class CreateModel : PageModel
    {
        private readonly PuzzleServerContext _context;

        [BindProperty]
        public Event Event { get; set; }

        public CreateModel(PuzzleServerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            // Ppoulate default fields
            Event = new Event();
            Event.TeamRegistrationBegin = DateTime.UtcNow;
            Event.TeamRegistrationEnd = DateTime.UtcNow.AddDays(1);
            Event.TeamNameChangeEnd = DateTime.UtcNow.AddDays(1);
            Event.TeamMembershipChangeEnd = DateTime.UtcNow.AddDays(1);
            Event.TeamMiscDataChangeEnd = DateTime.UtcNow.AddDays(1);
            Event.TeamDeleteEnd = DateTime.UtcNow.AddDays(1);
            Event.EventBegin = DateTime.UtcNow.AddDays(1);
            Event.AnswerSubmissionEnd= DateTime.UtcNow.AddDays(2);
            Event.AnswersAvailableBegin = DateTime.UtcNow.AddDays(2);
            Event.StandingsAvailableBegin = DateTime.UtcNow.AddDays(2);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Events.Add(Event);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}