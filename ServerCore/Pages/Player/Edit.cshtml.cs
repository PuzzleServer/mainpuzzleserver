using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Player
{
    // An empty Authorize attribute requires that the person is signed in but sets no other requirements
    [Authorize]
    public class EditModel : EventSpecificPageModel
    {
        public EditModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        [BindProperty]
        public PlayerInEvent PlayerInEvent { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlayerInEvent = await _context.PlayerInEvent
                .Include(p => p.Event)
                .Include(p => p.Player).FirstOrDefaultAsync(m => m.ID == id);

            if (PlayerInEvent == null)
            {
                return NotFound();
            }
           ViewData["EventId"] = new SelectList(_context.Events, "ID", "Name");
           ViewData["PlayerId"] = new SelectList(_context.PuzzleUsers, "ID", "IdentityUserId");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(PlayerInEvent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerInEventExists(PlayerInEvent.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Details", new { id = PlayerInEvent.ID });
        }

        private bool PlayerInEventExists(int id)
        {
            return _context.PlayerInEvent.Any(e => e.ID == id);
        }
    }
}
