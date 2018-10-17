﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PuzzleServerContext _context;
        public IList<Event> Events { get; set; }

        public IndexModel(PuzzleServerContext context)
        {
            _context = context;
        }
        public async Task OnGetAsync()
        {
            Events = await _context.Events.ToListAsync();
        }
    }
}
