using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Submissions
{
    public class IndexModel : PageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public IndexModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Submission Submission { get; set; }

        public IList<Submission> Submissions { get; set; }

        public int? PuzzleId { get; set; }

        public int? EventId { get; set; }

        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Submission.TimeSubmitted = DateTime.Now;
            Submission.Puzzle = await _context.Puzzles.SingleOrDefaultAsync(p => p.ID == puzzleId);

            List<Response> responses = await _context.Responses.Where(r => r.Puzzle.ID == puzzleId && Submission.SubmissionText == r.SubmittedText).ToListAsync();
            Submission.Response = responses.Count == 1 ? responses.First() : null;

            _context.Submissions.Add(Submission);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { puzzleid = puzzleId, eventid = Submission.Puzzle.Event.ID });
        }

        public async Task OnGetAsync(int? puzzleId, int? eventId)
        {
            if (puzzleId != null)
            {
                this.Submissions = await _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId).ToListAsync();
                this.PuzzleId = puzzleId;
            }
            else
            {
                this.Submissions = await _context.Submissions.ToListAsync();
            }

            if (eventId != null)
            {
                this.EventId = eventId;
            }
        }
    }
}