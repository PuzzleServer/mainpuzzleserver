using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly PuzzleServerContext _context;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender,
            PuzzleServerContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
        }

        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Employee alias")]
            public string EmployeeAlias { get; set; }
            public string Name { get; set; }

            [Display(Name = "T-shirt size")]
            public string TShirtSize { get; set; }

            [Display(Name = "User is visible to other users")]
            public bool VisibleToOthers { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUser(user.Id, _context);

            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Email = puzzleUser.EmailAddress,
                PhoneNumber = puzzleUser.PhoneNumber,
                EmployeeAlias = puzzleUser.EmployeeAlias,
                Name = puzzleUser.Name,
                TShirtSize = puzzleUser.TShirtSize,
                VisibleToOthers = puzzleUser.VisibleToOthers
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            PuzzleUser puzzleUser = PuzzleUser.GetPuzzleUser(user.Id, _context);

            if (Input.EmployeeAlias != puzzleUser.EmployeeAlias)
            {
                puzzleUser.EmployeeAlias = Input.EmployeeAlias;
            }

            if (Input.Name != puzzleUser.Name)
            {
                puzzleUser.Name = Input.Name;
            }

            if (Input.TShirtSize != puzzleUser.TShirtSize)
            {
                puzzleUser.TShirtSize = Input.TShirtSize;
            }

            if (Input.VisibleToOthers != puzzleUser.VisibleToOthers)
            {
                puzzleUser.VisibleToOthers = Input.VisibleToOthers;
            }

            if (Input.PhoneNumber != puzzleUser.PhoneNumber)
            {
                puzzleUser.PhoneNumber = Input.PhoneNumber;
            }

            if (Input.Email != puzzleUser.EmailAddress)
            {
                puzzleUser.EmailAddress = Input.Email;
            }

            // Note - this will consider all fields on the object to be modified. Extra work can be done here to have individual modification history if we need it.
            _context.Update(puzzleUser);
            await _context.SaveChangesAsync(true);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }


            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }
    }
}
