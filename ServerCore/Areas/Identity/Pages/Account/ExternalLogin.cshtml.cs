using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServerCore.DataModel;

namespace ServerCore.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly PuzzleServerContext _context;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger,
            PuzzleServerContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public PuzzleUser Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                _logger.LogError(ErrorMessage);
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                _logger.LogError($"{ErrorMessage}: ExternalLoginInfo is null");
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // If this providerKey can't be found, it may mean that the providerKey for an existing user has been changed by the provider.
            // This is fine, and just means that the new providerKey needs to be added as an extra login for the existing user.
            // Use the email as a lookup for the existing user, and add the login there.
            // Note that FindByEmailAsync says that in order to use it, you should also set IdentityOptions.User.RequireUniqueEmail = true; see Startup.cs.
            if (await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey) == null && info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                var existingUser = await _userManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
                if (existingUser != null)
                {
                    await _userManager.AddLoginAsync(existingUser, info);
                    _logger.LogInformation("User's providerKey has been changed by the provider, new providerKey added.");
                }
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                LoginProvider = info.LoginProvider;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new PuzzleUser
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                        IdentityUserId = "fake"
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                _logger.LogError($"{ErrorMessage}: ExternalLoginInfo is null");
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        Input.IdentityUserId = user.Id;

                        if (ModelState.IsValid)
                        {
                            // If there are no GlobalAdmins make them the GlobalAdmin
                            if ((Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development) && !_context.PuzzleUsers.Where(u => u.IsGlobalAdmin).Any())
                            {
                                Input.IsGlobalAdmin = true;
                            }

                            _context.PuzzleUsers.Add(Input);
                            await _context.SaveChangesAsync(true);
                            transaction.Commit();

                            await _signInManager.SignInAsync(user, isPersistent: false);
                            _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                            return LocalRedirect(returnUrl);
                        }
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            LoginProvider = info.LoginProvider;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
