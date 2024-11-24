using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ServerCore.Pages
{
    public class ErrorPageModel : PageModel
    {
        public string ErrorMessage { get; set; }

        public void OnGet(string returnUrl, string errorMessage)
        {
            this.ErrorMessage = errorMessage;
        }
    }
}
