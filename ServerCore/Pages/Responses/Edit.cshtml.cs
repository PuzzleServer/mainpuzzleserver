using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Models;

namespace ServerCore.Pages.Responses
{
    public class EditModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public EditModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Response Response { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Response = await _context.Responses.FirstOrDefaultAsync(m => m.ID == id);

            if (Response == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Response).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResponseExists(Response.ID))
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

        private bool ResponseExists(int id)
        {
            return _context.Responses.Any(e => e.ID == id);
        }
    }
}
