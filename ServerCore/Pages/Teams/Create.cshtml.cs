using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ServerCore.Areas.Identity;
using ServerCore.DataModel;
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

            if (EventRole != EventRole.play && EventRole != EventRole.admin)
            {
                return Forbid();
            }

            if (EventRole == EventRole.play && GetTeamId().Result != -1)
            {
                return Forbid();
            }

            if (EventRole == EventRole.admin && !await LoggedInUser.IsAdminForEvent(_context, Event))
            {
                return Forbid();
            }

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
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if (EventRole != EventRole.play && EventRole != EventRole.admin)
            {
                return NotFound();
            }

            if (EventRole == EventRole.admin && !await LoggedInUser.IsAdminForEvent(_context, Event))
            {
                return Forbid();
            }

            if (!Event.IsTeamRegistrationActive)
            {
                return NotFound();
            }

            ModelState.Remove("Team.Event");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Team.Event = Event;

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                if (await _context.Teams.Where((t) => t.Event == Event).CountAsync() >= Event.MaxNumberOfTeams)
                {
                    return NotFound("Registration is full. No further teams may be created at the present time.");
                }

                _context.Teams.Add(Team);

                if (EventRole == EventRole.play)
                {
                    if (await (from member in _context.TeamMembers
                               where member.Member == LoggedInUser &&
                               member.Team.Event == Event
                               select member).AnyAsync())
                    {
                        return NotFound("You are already on a team. Leave that team before creating a new one.");
                    }

                    TeamMembers teamMember = new TeamMembers()
                    {
                        Team = Team,
                        Member = LoggedInUser
                    };
                    _context.TeamMembers.Add(teamMember);
                }

                var hints = from Hint hint in _context.Hints
                            where hint.Puzzle.Event == Event
                            select hint;

                foreach (Hint hint in hints)
                {
                    _context.HintStatePerTeam.Add(new HintStatePerTeam() { Hint = hint, Team = Team });
                }

                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            if(EventRole == ModelBases.EventRole.play)
            {
                return RedirectToPage("./Details");
            }
            return RedirectToPage("./Index");
        }
    }
}