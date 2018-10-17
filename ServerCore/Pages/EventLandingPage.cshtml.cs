using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages
{
    public class EventLandingPageModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public EventLandingPageModel(PuzzleServerContext context)
        {
            _context = context;
        }
        public void OnGet()
        {

        }
    }
}
