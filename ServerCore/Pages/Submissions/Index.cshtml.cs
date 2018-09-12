using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Submissions
{
    public class IndexModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public IndexModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Submission Submission { get; set; }

        public IList<Submission> Submissions { get; set; }

        public int PuzzleId { get; set; }

        public string AnswerToken { get; set; }

        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            Submission.TimeSubmitted = DateTime.Now;
            Submission.Puzzle = await _context.Puzzles.SingleOrDefaultAsync(p => p.ID == puzzleId);

            List<Response> responses = await _context.Responses.Where(r => r.Puzzle.ID == puzzleId && Submission.SubmissionText == r.SubmittedText).ToListAsync();
            Submission.Response = responses.Count == 1 ? responses.First() : null;

            _context.Submissions.Add(Submission);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { puzzleid = puzzleId });
        }

        public async Task OnGetAsync(int puzzleId)
        {
            this.Submissions = await _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId).ToListAsync();
            this.PuzzleId = puzzleId;

            Submission lastSubmission = this.Submissions.LastOrDefault();
            if (lastSubmission != null && lastSubmission.Response != null && lastSubmission.Response.IsSolution)
            {
                this.AnswerToken = lastSubmission.SubmissionText;
            }
        }
    }
}