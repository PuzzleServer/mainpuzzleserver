using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class CreateBulkMultiModel : EventSpecificPageModel
    {
        public CreateBulkMultiModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public bool DeleteExisting { get; set; }

        [BindProperty]
        public string PuzzleName { get; set; }

        [BindProperty]
        public string IsSolution { get; set; }

        [BindProperty]
        public string SubmittedText { get; set; }

        [BindProperty]
        public string ResponseText { get; set; }

        [BindProperty]
        public string Note { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            using StringReader puzzleNameReader = new StringReader(PuzzleName ?? string.Empty);
            using StringReader isSolutionReader = new StringReader(IsSolution ?? string.Empty);
            using StringReader submittedTextReader = new StringReader(SubmittedText ?? string.Empty);
            using StringReader responseTextReader = new StringReader(ResponseText ?? string.Empty);
            using StringReader noteReader = new StringReader(Note ?? string.Empty);

            Dictionary<string, int> puzzleTitleLookup = new Dictionary<string, int>();
            Dictionary<string, HashSet<string>> submissionsLookup = new Dictionary<string, HashSet<string>>();

            while (true)
            {
                string puzzleName = puzzleNameReader.ReadLine();
                string isSolution = isSolutionReader.ReadLine();
                string submittedText = submittedTextReader.ReadLine();
                string responseText = responseTextReader.ReadLine();
                string note = noteReader.ReadLine();

                int puzzleId = -1;

                if (puzzleName != null && !puzzleTitleLookup.TryGetValue(puzzleName, out puzzleId))
                {
                    Puzzle puzzle = await (from Puzzle p in _context.Puzzles
                                           where p.PlaintextName == puzzleName && p.EventID == this.Event.ID
                                           select p).FirstOrDefaultAsync();

                    if (puzzle == null)
                    {
                        ModelState.AddModelError("PuzzleName", $"Puzzle '{puzzleName}' not found");
                        break;
                    }

                    if (EventRole == EventRole.author && !await UserEventHelper.IsAuthorOfPuzzle(_context, puzzle, LoggedInUser))
                    {
                        ModelState.AddModelError("PuzzleName", $"Not an author of puzzle '{puzzleName}'");
                        break;
                    }

                    puzzleId = puzzle.ID;

                    puzzleTitleLookup[puzzleName] = puzzleId;
                    submissionsLookup[puzzleName] = new HashSet<string>();

                    if (DeleteExisting)
                    {
                        Response[] responsesToRemove = await (from Response r in _context.Responses
                                                              where r.PuzzleID == puzzleId
                                                              select r).ToArrayAsync();
                        _context.Responses.RemoveRange(responsesToRemove);
                    }
                    else
                    {
                        string[] responses = await (from Response r in _context.Responses
                               where r.PuzzleID == puzzleId
                               select r.SubmittedText).ToArrayAsync();
                        foreach (string r in responses)
                        {
                            submissionsLookup[puzzleName].Add(r);
                        }
                    }
                }

                // TODO probably clearer ways to validate but I honestly do not understand how validation works
                if (puzzleName == null)
                {
                    if (submittedText != null)
                    {
                        ModelState.AddModelError("SubmittedText", "Unmatched Submission without Puzzle");
                    }
                    if (responseText != null)
                    {
                        ModelState.AddModelError("ResponseText", "Unmatched Response without Puzzle");
                    }
                    if (isSolution != null)
                    {
                        ModelState.AddModelError("IsSolution", "Unmatched IsSolution without Puzzle");
                    }
                    if (note != null)
                    {
                        ModelState.AddModelError("Note", "Unmatched Note without Puzzle");
                    }

                    // we're done
                    break;
                }

                if (submittedText == null)
                {
                    ModelState.AddModelError("SubmittedText", "Unmatched Puzzle without Submission");
                    break;
                }

                string submittedTextFormatted = ServerCore.DataModel.Response.FormatSubmission(submittedText);

                // Ensure that the submission text is unique for this puzzle.
                if (!submissionsLookup[puzzleName].Add(submittedTextFormatted))
                {
                    ModelState.AddModelError("SubmittedText", "Submission text is not unique");
                    break;
                }

                if (responseText == null)
                {
                    ModelState.AddModelError("SubmittedText", "Unmatched Puzzle without Response");
                    break;
                }

                isSolution = isSolution == null ? string.Empty : isSolution.ToLower();

                if (puzzleId == -1)
                {
                    throw new Exception($"Bug in puzzleId lookup for {puzzleName}");
                }

                Response response = new Response()
                {
                    PuzzleID = puzzleId,
                    SubmittedText = submittedText,
                    ResponseText = responseText,
                    Note = note,
                    IsSolution = isSolution == "y" || isSolution == "yes" || isSolution == "t" || isSolution == "true" || isSolution == "1"
                };

                _context.Responses.Add(response);
            }

            if (!ModelState.IsValid)
            {
                return OnGet();
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/Puzzles/Index");
        }
    }
}