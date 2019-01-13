using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
{
    public class DetailsModel : EventSpecificPageModel
    {
        public DetailsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

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
    }
}
