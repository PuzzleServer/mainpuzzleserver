using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Events
{
    /// <summary>
    /// Page for manually applying data changes to the database when they're not easily scripted
    /// </summary>
    [Authorize(Policy = "IsGlobalAdmin")]
    public class MigrateDataModel : PageModel
    {
        private readonly PuzzleServerContext _context;

        public string Status { get; set; }

        public MigrateDataModel(PuzzleServerContext context, UserManager<IdentityUser> manager)
        {
            _context = context;
        }

        public void OnGet()
        {

        }

        /// <summary>
        /// Sets the MayBeAdminOrAuthor bit for all admins and authors of all events
        /// </summary>
        public async Task<IActionResult> OnPostMigrateAdminsAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable))
            {
                List<PuzzleUser> allAdmins = await (from admin in _context.EventAdmins
                                                    select admin.Admin).ToListAsync();
                List<PuzzleUser> allAuthors = await (from author in _context.EventAuthors
                                                     select author.Author).ToListAsync();

                foreach (PuzzleUser admin in allAdmins)
                {
                    admin.MayBeAdminOrAuthor = true;
                }
                foreach (PuzzleUser author in allAuthors)
                {
                    author.MayBeAdminOrAuthor = true;
                }

                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            Status = "Admins and authors migrated";

            return Page();
        }

        /// <summary>
        /// Deletes all duplicate submissions from the database
        /// </summary>
        public async Task<IActionResult> OnPostDeleteDuplicateSubmissionsAsync()
        {
            using (var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable))
            {
                var duplicateSubGroups = await (from submission in _context.Submissions
                                                group submission by new { submission.PuzzleID, submission.TeamID, submission.SubmissionText } into g
                                                where g.Count() > 1
                                                select (from sub in g select sub.ID).ToArray()).ToListAsync();

                List<Submission> submissionsToDelete = new List<Submission>();
                foreach(int[] dupeGroup in duplicateSubGroups)
                {
                    for (int i = 0; i < dupeGroup.Length - 1; i++)
                    {
                        int subId = dupeGroup[i];
                        Submission toDelete = new Submission() { ID = subId };
                        _context.Attach(toDelete);
                        submissionsToDelete.Add(toDelete);
                    }
                }
                _context.Submissions.RemoveRange(submissionsToDelete);
                await _context.SaveChangesAsync();
                transaction.Commit();

                Status = $"{submissionsToDelete.Count} duplicates removed";
            }

            return Page();
        }

        /// <summary>
        /// Create missing PuzzleStatePerTeam rows from when they were lazily created
        /// </summary>
        public async Task<IActionResult> OnPostCreateMissingPSPTAsync()
        {
            List<int> allEvents = await (from ev in _context.Events
                                         select ev.ID).ToListAsync();

            var allPspts = await (from pspt in _context.PuzzleStatePerTeam
                                  select new { pspt.PuzzleID, pspt.TeamID }).ToDictionaryAsync(pspt => (((ulong)pspt.PuzzleID) << 32) | (uint)pspt.TeamID);

            List<PuzzleStatePerTeam> newPspts = new List<PuzzleStatePerTeam>();

            foreach (int ev in allEvents)
            {
                List<int> allTeamPuzzles = await (from puzzle in _context.Puzzles
                                              where puzzle.EventID == ev && !puzzle.IsForSinglePlayer
                                              select puzzle.ID).ToListAsync();
                List<int> allTeams = await (from team in _context.Teams
                                            where team.EventID == ev
                                            select team.ID).ToListAsync();

                foreach (int puzzle in allTeamPuzzles)
                {
                    foreach (int team in allTeams)
                    {
                        ulong key = (((ulong)puzzle) << 32) | (uint)team;
                        if (!allPspts.ContainsKey(key))
                        {
                            PuzzleStatePerTeam newPspt = new PuzzleStatePerTeam()
                            {
                                PuzzleID = puzzle,
                                TeamID = team,
                            };

                            newPspts.Add(newPspt);
                        }
                    }
                }
            }
            if (newPspts.Count > 0)
            {
                _context.PuzzleStatePerTeam.AddRange(newPspts.ToArray());
                await _context.SaveChangesAsync();
                Status = $"Added {newPspts.Count} missing PuzzleStatePerTeam rows";
            }
            else
            {
                Status = "No new PuzzleStatePerTeam rows needed";
            }

            return Page();
        }
    }
}