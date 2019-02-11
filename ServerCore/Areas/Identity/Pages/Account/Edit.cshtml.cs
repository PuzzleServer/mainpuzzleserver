using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class EditModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PuzzleServerContext _context;

        public EditModel(
            UserManager<IdentityUser> userManager,
            PuzzleServerContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public PuzzleUser PuzzleUser { get; set; }

        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int userId, string returnUrl = null)
        {
            var thisPuzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(_context, User, _userManager);

            if (thisPuzzleUser == null)
            {
                return Forbid();
            }

            PuzzleUser = await _context.PuzzleUsers.Where((p) => p.ID == userId).FirstOrDefaultAsync();

            if (thisPuzzleUser.ID != PuzzleUser.ID && !thisPuzzleUser.IsGlobalAdmin)
            {
                return Forbid();
            }

            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                // TODO: For some reason the ID property is not reported as valid. No clue how validation works and all other properties are strings anyway. Ignore for now?
                return Page();
            }

            var thisPuzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(_context, User, _userManager);

            if (thisPuzzleUser == null)
            {
                return Forbid();
            }

            if (thisPuzzleUser.ID != PuzzleUser.ID && !thisPuzzleUser.IsGlobalAdmin)
            {
                return Forbid();
            }

            _context.Entry(thisPuzzleUser).State = EntityState.Detached;
            _context.Attach(PuzzleUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return Redirect(returnUrl);
        }
    }
}