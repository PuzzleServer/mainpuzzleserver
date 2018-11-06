using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class CreateModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public CreateModel(PuzzleServerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {        
            if (!Event.IsTeamRegistrationActive)
            {
                return NotFound();
            }

            return Page();
        }

        [BindProperty]
        public Team Team { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!Event.IsTeamRegistrationActive)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Team.Event = Event;

            _context.Teams.Add(Team);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}