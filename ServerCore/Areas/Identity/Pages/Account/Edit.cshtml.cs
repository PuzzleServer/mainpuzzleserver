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
    [Authorize(Policy = "IsGlobalAdmin")]
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
            PuzzleUser = await _context.PuzzleUsers.Where((p) => p.ID == userId).FirstOrDefaultAsync();
            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!MailHelper.IsValidEmail(PuzzleUser.Email))
            {
                ModelState.AddModelError("PuzzleUser.Email", "This email address is not valid.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var thisPuzzleUser = await PuzzleUser.GetPuzzleUserForCurrentUser(_context, User, _userManager);

            if (thisPuzzleUser != null)
            {
                _context.Entry(thisPuzzleUser).State = EntityState.Detached;
            }

            _context.Attach(PuzzleUser).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return Redirect(returnUrl);
        }
    }
}