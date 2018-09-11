using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Models;

namespace ServerCore.Pages.Responses
{
    public class DeleteModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public DeleteModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Response PuzzleResponse { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PuzzleResponse = await _context.Responses.FirstOrDefaultAsync(m => m.ID == id);

            if (Response == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PuzzleResponse = await _context.Responses.FindAsync(id);

            if (Response != null)
            {
                _context.Responses.Remove(PuzzleResponse);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
