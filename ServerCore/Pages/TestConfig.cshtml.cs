using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServerCore.Pages
{
    [Authorize(Policy = "IsGlobalAdmin")]
    public class TestConfigModel : PageModel
    {
        public IActionResult OnPostSendTestEmail()
        {
            // The purpose of this function is to verify that the server is configured properly.
            // That's why there are no options and we don't try different kinds of recipients and bodies.
            // The one exception is including a hyperlink in the test mail so we can verify linkifying
            // happens in various email clients.
            if (MailHelper.Singleton == null)
            {
                return StatusCode(500, "MailHelper was not initialized.");
            }
            MailHelper.Singleton.SendPlaintextBcc(
                new List<string> { "puzztech@service.microsoft.com" },
                "This is a test from PuzzleServer",
                "This test is from https://github.com/PuzzleServer/mainpuzzleserver");
            if (!MailHelper.Singleton.Enabled)
            {
                return StatusCode(500, "MailHelper was initialized, but not configured properly. Mail sent to debug output.");
            }
            return RedirectToPage();
        }
    }
}
