using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
{
    public class DeleteModel : EventSpecificPageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public DeleteModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Hint = await _context.Hints.FindAsync(id);

            if (Hint != null)
            {
                _context.Hints.Remove(Hint);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
