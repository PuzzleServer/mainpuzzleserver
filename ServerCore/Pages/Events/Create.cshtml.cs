using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsGlobalAdmin")]
    public class CreateModel : PageModel
    {
        private readonly PuzzleServerContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        const string SharedResourceDirectoryName = "resources";

        [BindProperty]
        public Event Event { get; set; }

        public CreateModel(PuzzleServerContext context, UserManager<IdentityUser> manager)
        {
            _context = context;
            _userManager = manager;
        }

        public IActionResult OnGet()
        {
            // Populate default fields
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
            Event.LunchReportDate = DateTime.UtcNow.AddDays(1);
            Event.LockoutIncorrectGuessLimit = 5;
            Event.LockoutIncorrectGuessPeriod = 15;
            Event.LockoutDurationMultiplier = 2;
            Event.MaxSubmissionCount = 50;
            Event.MaxNumberOfTeams = 120;
            Event.MaxExternalsPerTeam = 9;
            Event.MaxTeamSize = 12;
            Event.EmbedPuzzles = true;
            Event.EventPassword = Guid.NewGuid().ToString();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Events.Add(Event);

            var loggedInUser = await PuzzleUser.GetPuzzleUserForCurrentUser(_context, User, _userManager);

            if (loggedInUser != null)
            {
                _context.EventAdmins.Add(new EventAdmins() { Event = Event, Admin = loggedInUser });
                _context.EventAuthors.Add(new EventAuthors() { Event = Event, Author = loggedInUser });
                loggedInUser.MayBeAdminOrAuthor = true;
            }

            await _context.SaveChangesAsync();

            // Create base files for event content and style
            // Fails silently if local Azure storage emulator isn't installed
            NewFileCreationHelper.CreateNewEventFiles(Event.ID, SharedResourceDirectoryName);

            return RedirectToPage("./Index");
        }
    }
}

