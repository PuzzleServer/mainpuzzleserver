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
        private readonly ServerCore.DataModel.PuzzleServerContext _context;

        public IndexModel(ServerCore.DataModel.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Submission Submission { get; set; }

        public IList<Submission> Submissions { get; set; }

        public int PuzzleId { get; set; }

        public int TeamId { get; set; }

        public string AnswerToken { get; set; }

        public async Task<IActionResult> OnPostAsync(int puzzleId, int teamId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!this.Event.IsAnswerSubmissionActive)
            {
                return RedirectToPage("/Submissions/Index", new { puzzleid = puzzleId, teamid = teamId });
            }

            // Create submission and add it to list
            Submission.TimeSubmitted = DateTime.Now;
            Submission.Puzzle = await _context.Puzzles.SingleOrDefaultAsync(p => p.ID == puzzleId);
            Submission.Team = await _context.Teams.Where((t) => t.ID == teamId).FirstOrDefaultAsync();

            List<Response> responses = await _context.Responses.Where(r => r.Puzzle.ID == puzzleId && Submission.SubmissionText == r.SubmittedText).ToListAsync();
            Submission.Response = responses.FirstOrDefault();

            // Update puzzle state if submission was correct
            if (Submission.Response != null && Submission.Response.IsSolution)
            {
                var statesQ = await PuzzleStateHelper.GetFullReadWriteQueryAsync(_context, this.Event, Submission.Puzzle, Submission.Team);
                PuzzleStatePerTeam puzzleState = await statesQ.FirstOrDefaultAsync();
                puzzleState.IsSolved = true;
                Submission.TimeSubmitted = (DateTime)puzzleState.SolvedTime;
            }
            
            _context.Submissions.Add(Submission);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Submissions/Index", new { puzzleid = puzzleId, teamid = teamId });
        }

        public async Task OnGetAsync(int puzzleId, int teamId)
        {
            Submissions = await _context.Submissions.Where((s) => s.Team != null && s.Team.ID == teamId && s.Puzzle != null && s.Puzzle.ID == puzzleId).ToListAsync();
            PuzzleId = puzzleId;
            TeamId = teamId;

            Submission correctSubmission = this.Submissions?.Where((s) => s.Response != null && s.Response.IsSolution).FirstOrDefault();
            if (correctSubmission != null)
            {
                AnswerToken = correctSubmission.SubmissionText;
            }
        }
    }
}