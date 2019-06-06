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
                                                select new { g.Key, Count = g.Count() }).ToListAsync();

                List<Task<int>> allFetches = new List<Task<int>>();
                foreach (var dupeGroup in duplicateSubGroups)
                {
                    for (int i = 0; i < dupeGroup.Count - 1; i++)
                    {
                        Task<int> t = (from submission in _context.Submissions
                                       where submission.PuzzleID == dupeGroup.Key.PuzzleID &&
                                       submission.TeamID == dupeGroup.Key.TeamID &&
                                       submission.SubmissionText == dupeGroup.Key.SubmissionText
                                       select submission.ID).Skip(i).FirstAsync();
                        allFetches.Add(t);
                    }
                }

                await Task.WhenAll(allFetches);

                List<Submission> submissionsToDelete = new List<Submission>();
                foreach(Task<int> fetch in allFetches)
                {
                    int subId = fetch.Result;
                    Submission toDelete = new Submission() { ID = subId };
                    _context.Attach(toDelete);
                    submissionsToDelete.Add(toDelete);
                }
                _context.Submissions.RemoveRange(submissionsToDelete);
                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            return Page();
        }
    }
}