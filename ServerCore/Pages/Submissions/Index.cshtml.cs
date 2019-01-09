using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Submissions
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class IndexModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext bdContext;

        public IndexModel(PuzzleServerContext context)
        {
            bdContext = context;
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
            Submission.TimeSubmitted = DateTime.UtcNow;
            Submission.Puzzle = await bdContext.Puzzles.SingleOrDefaultAsync(p => p.ID == puzzleId);
            Submission.Team = await bdContext.Teams.Where((t) => t.ID == teamId).FirstOrDefaultAsync();

            List<Response> responses = await bdContext.Responses.Where(r => r.Puzzle.ID == puzzleId && Submission.SubmissionText == r.SubmittedText).ToListAsync();
            Submission.Response = responses.FirstOrDefault();

            // Update puzzle state if submission was correct
            if (Submission.Response != null && Submission.Response.IsSolution)
            {
                await PuzzleStateHelper.SetSolveStateAsync(bdContext, Event, Submission.Puzzle, Submission.Team, Submission.TimeSubmitted);
            }
            
            bdContext.Submissions.Add(Submission);
            await bdContext.SaveChangesAsync();

            return RedirectToPage("/Submissions/Index", new { puzzleid = puzzleId, teamid = teamId });
        }

        public async Task OnGetAsync(int puzzleId, int teamId)
        {
            Submissions = await bdContext.Submissions.Where((s) => s.Team != null && s.Team.ID == teamId && s.Puzzle != null && s.Puzzle.ID == puzzleId).ToListAsync();
            PuzzleId = puzzleId;
            TeamId = teamId;

            Submission correctSubmission = Submissions?.Where((s) => s.Response != null && s.Response.IsSolution).FirstOrDefault();
            if (correctSubmission != null)
            {
                AnswerToken = correctSubmission.SubmissionText;
            }
        }
    }
}