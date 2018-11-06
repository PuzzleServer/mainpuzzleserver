using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
{
    public class CreateModel : EventSpecificPageModel
    {
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public CreateModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {

            //Hint = new Hint()
            //{
            //    Puzzle = puzzle
            //};

            return Page();
        }

        [BindProperty]
        public Hint Hint { get; set; }

        public async Task<IActionResult> OnPostAsync(int puzzleID)
        {
            Puzzle puzzle = (from puzz in _context.Puzzles
                             where puzz.ID == puzzleID
                             select puzz).SingleOrDefault();
            if (puzzle == null)
            {
                return NotFound();
            }

            Hint.Puzzle = puzzle;
            ModelState.Remove("Hint.Puzzle");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Hints.Add(Hint);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}