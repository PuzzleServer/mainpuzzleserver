using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCore.DataModel;

namespace ServerCore.Pages.Submissions
{
    public class FreeformQueueModel : PageModel
    {
        public Puzzle Puzzle { get; set; }

        public void OnGet()
        {
        }
    }
}
