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
    public class DetailsModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public DetailsModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

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
    }
}
