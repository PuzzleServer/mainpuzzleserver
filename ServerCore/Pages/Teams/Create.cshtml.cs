using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [AllowAnonymous]
    public class CreateModel : EventSpecificPageModel
    {
        public CreateModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if ((EventRole != EventRole.play && EventRole != EventRole.admin)
                || IsNotAllowedInInternEvent()
                || (EventRole == EventRole.admin && !await LoggedInUser.IsAdminForEvent(_context, Event)))
            {
                return Forbid();
            }

            if (EventRole == EventRole.play && GetTeamId().Result != -1)
            {
                return NotFound("You are already on a team and cannot create a new one.");
            }

            if (!Event.IsTeamRegistrationActive && EventRole != EventRole.admin)
            {
                return NotFound();
            }

            return Page();
        }

        [BindProperty]
        public Team Team { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if (EventRole != EventRole.play && EventRole != EventRole.admin)
            {
                return NotFound();
            }

            if ((EventRole == EventRole.admin && !await LoggedInUser.IsAdminForEvent(_context, Event))
                || (EventRole != EventRole.admin && IsNotAllowedInInternEvent()))
            {
                return Forbid();
            }

            if (!Event.IsTeamRegistrationActive && EventRole != EventRole.admin)
            {
                return NotFound();
            }

            if (EventRole != EventRole.admin && await (from member in _context.TeamMembers
                       where member.Member == LoggedInUser &&
                       member.Team.Event == Event
                       select member).AnyAsync())
            {
                return NotFound("You are already on a team. Leave that team before creating a new one.");
            }

            if (string.IsNullOrWhiteSpace(Team.PrimaryContactEmail))
            {
                ModelState.AddModelError("Team.PrimaryContactEmail", "An email is required.");
            }
            else if (!MailHelper.IsValidEmail(Team.PrimaryContactEmail))
            {
                ModelState.AddModelError("Team.PrimaryContactEmail", "This email address is not valid.");
            }

            Team.Name = TeamHelper.UnicodeSanitizeTeamName(Team.Name);
            if (string.IsNullOrWhiteSpace(Team.Name))
            {
                ModelState.AddModelError("Team.Name", "Team names cannot be left blank.");
            }

            if (TeamHelper.IsTeamNameTaken(_context, Event.ID, Team.Name))
            {
                ModelState.AddModelError("Team.Name", "Another team has this name.");
            }

            ModelState.Remove("Team.Event");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (EventRole != EventRole.admin && await _context.Teams.Where((t) => t.Event == Event).CountAsync() >= Event.MaxNumberOfTeams)
            {
                return NotFound("Registration is full. No further teams may be created at the present time.");
            }

            int? idToAdd = null;
            if (EventRole == EventRole.play)
            {
                idToAdd = LoggedInUser.ID;
            }
            await TeamHelper.CreateTeamAsync(_context, Team, Event, idToAdd);

            int teamId = await GetTeamId();
            if (EventRole == ModelBases.EventRole.play)
            {
                return RedirectToPage("./Details", new { teamId = teamId });
            }
            return RedirectToPage("./Index");
        }
    }
}