using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class EditModel : EventSpecificPageModel
    {
        public EditModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public Hint Hint { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Hint = await _context.Hints.FirstOrDefaultAsync(m => m.Id == id);

            if (Hint == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // The Puzzle property doesn't get round-tripped by ASP.NET and would cause
            // the validation below to fail. By removing it from the ModelState,
            // validation passes.
            ModelState.Remove("Hint.Puzzle");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Hint).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                var puzzleName = await _context.Hints.Where(m => m.Id == Hint.Id).Select(m => m.Puzzle.Name).FirstOrDefaultAsync();

                var teamMembers = await (from TeamMembers tm in _context.TeamMembers
                                         join HintStatePerTeam hspt in _context.HintStatePerTeam on tm.Team equals hspt.Team
                                         where hspt.Hint.Id == Hint.Id
                                         select tm.Member.Email).ToListAsync();
                MailHelper.Singleton.SendPlaintextBcc(teamMembers,
                    $"Hint updated for '{Hint.Description}' on {puzzleName}",
                    $"The new content for this hint is: '{Hint.Content}'.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HintExists(Hint.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool HintExists(int id)
        {
            return _context.Hints.Any(e => e.Id == id);
        }
    }
}
