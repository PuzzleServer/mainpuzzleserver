using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    [Authorize("IsEventAdmin")]
    public class AddAdminOrAuthorModel : EventSpecificPageModel
    {
        public IList<AddUserHelper.PuzzleUserView> Users { get; set; }

        public bool addAdmin { get; set; }

        public AddAdminOrAuthorModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(bool addAdmin)
        {
            this.addAdmin = addAdmin;

            Users = addAdmin ? await AddUserHelper.GetNonAdminsForEvent(_context, Event) : await AddUserHelper.GetNonAuthorsForEvent(_context, Event);

            return Page();
        }

        public async Task<IActionResult> OnGetAddAdminAsync(int userId)
        {
            // Check that the user isn't already an admin
            if (await (from admin in _context.EventAdmins
                       where admin.Admin.ID == userId &&
                       admin.Event == Event
                       select admin).AnyAsync())
            {
                return NotFound("User is already an admin in this event.");
            }

            EventAdmins Admin = new EventAdmins();

            PuzzleUser user = await _context.PuzzleUsers.FirstOrDefaultAsync(m => m.ID == userId);
            if (user == null)
            {
                return NotFound("Could not find user with ID '" + userId + "'. Check to make sure the user hasn't been removed.");
            }
            Admin.Admin = user;

            Admin.Event = Event;

            _context.EventAdmins.Add(Admin);
            user.MayBeAdminOrAuthor = true;

            await _context.SaveChangesAsync();
            return RedirectToPage("/Events/Players");
        }

        public async Task<IActionResult> OnGetAddAuthorAsync(int userId)
        {
            // Check that the user isn't already an author
            if (await (from author in _context.EventAuthors
                       where author.Author.ID == userId &&
                       author.Event == Event
                       select author).AnyAsync())
            {
                return NotFound("User is already an author in this event.");
            }

            EventAuthors Author = new EventAuthors();

            PuzzleUser user = await _context.PuzzleUsers.FirstOrDefaultAsync(m => m.ID == userId);
            if (user == null)
            {
                return NotFound("Could not find user with ID '" + userId + "'. Check to make sure the user hasn't been removed.");
            }
            Author.Author = user;

            Author.Event = Event;

            _context.EventAuthors.Add(Author);
            user.MayBeAdminOrAuthor = true;

            await _context.SaveChangesAsync();
            return RedirectToPage("/Events/Players");
        }
    }
}