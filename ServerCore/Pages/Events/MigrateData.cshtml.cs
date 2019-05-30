using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Events
{
    /// <summary>
    /// Page for manually applying data changes to the database when they're not easily scripted
    /// </summary>
    [Authorize(Policy = "IsGlobalAdmin")]
    public class MigrateDataModel : PageModel
    {
        private readonly PuzzleServerContext _context;

        public MigrateDataModel(PuzzleServerContext context, UserManager<IdentityUser> manager)
        {
            _context = context;
        }

        public void OnGet()
        {

        }

        /// <summary>
        /// Sets the MayBeAdminOrAuthor bit for all admins and authors of all events
        /// </summary>
        public async Task<IActionResult> OnPostMigrateAdminsAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable))
            {
                List<PuzzleUser> allAdmins = await (from admin in _context.EventAdmins
                                 select admin.Admin).ToListAsync();
                List<PuzzleUser> allAuthors = await (from author in _context.EventAuthors
                                                     select author.Author).ToListAsync();

                foreach(PuzzleUser admin in allAdmins)
                {
                    admin.MayBeAdminOrAuthor = true;
                }
                foreach (PuzzleUser author in allAuthors)
                {
                    author.MayBeAdminOrAuthor = true;
                }

                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            return Page();
        }
    }
}