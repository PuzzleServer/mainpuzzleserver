using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;
using static ServerCore.ModelBases.EventSpecificPageModel;

namespace ServerCore.Helpers
{

    public enum SubmissionResponseCode
    {
        Correct,
        Incorrect,
        Freeform,
        Partial,
        Unauthorized,
        PuzzleNotFound,
        PuzzleLocked,
        EmptySubmission
    }

    public class SubmissionEvaluator
    {
        public PuzzleStatePerTeam PuzzleState { get; set; }

        public string SubmissionText { get; set; }

        public List<SubmissionView> SubmissionViews { get; set; }

        public Puzzle Puzzle { get; set; }

        public Team Team { get; set; }

        public string AnswerToken { get; set; }

        public IList<Puzzle> PuzzlesCausingGlobalLockout { get; set; }

        public bool DuplicateSubmission { get; set; }

        public bool AllowFreeformSharing { get; set; }

        private IList<Submission> Submissions { get; set; }

        private static PuzzleServerContext _context;

        public class SubmissionView
        {
            public Submission Submission { get; set; }
            public Response Response { get; set; }
            public string SubmitterName { get; set; }
            public bool IsFreeform { get; set; }
            public string FreeformReponse { get; set; }
        }

        public static async Task<SubmissionResponseCode> EvaluateSubmission(PuzzleServerContext context, string submissionText)
        {
            if (String.IsNullOrWhiteSpace(submissionText))
            {
                return SubmissionResponseCode.EmptySubmission;
            }

            string SubmissionText = submissionText;

            Team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);

            Puzzle = await _context.Puzzles.Where(
                (p) => p.ID == puzzleId).FirstOrDefaultAsync();

            if (DateTime.UtcNow < Event.EventBegin)
            {
                return SubmissionResponseCode.Unauthorized;
            }

            PuzzleState = await (PuzzleStateHelper
      .GetFullReadOnlyQuery(
          _context,
          Event,
          Puzzle,
          Team))
      .FirstAsync();

        }

       
    }
}
