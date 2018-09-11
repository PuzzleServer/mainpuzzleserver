using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Models;

namespace ServerCore.Pages.Submissions
{
    public class DetailsModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public DetailsModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public Submission Submission { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Submission = await _context.Submissions.FirstOrDefaultAsync(m => m.ID == id);

            if (Submission == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
