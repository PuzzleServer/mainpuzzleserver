using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
{
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
