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
using ServerCore.ModelBases;

namespace ServerCore.Pages.Responses
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class CreateBulkModel : EventSpecificPageModel
    {
        public CreateBulkModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public string IsSolution { get; set; }

        [BindProperty]
        public string SubmittedText { get; set; }

        [BindProperty]
        public string ResponseText { get; set; }

        [BindProperty]
        public string Note { get; set; }

        public int PuzzleId { get; set; }

        public Puzzle Puzzle { get; set; }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            PuzzleId = puzzleId;
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            StringReader isSolutionReader = new StringReader(IsSolution ?? string.Empty);
            StringReader submittedTextReader = new StringReader(SubmittedText ?? string.Empty);
            StringReader responseTextReader = new StringReader(ResponseText ?? string.Empty);
            StringReader noteReader = new StringReader(Note ?? string.Empty);

            HashSet<string> submissions = new HashSet<string>();

            while (true)
            {
                string isSolution = isSolutionReader.ReadLine();
                string submittedText = submittedTextReader.ReadLine();
                string responseText = responseTextReader.ReadLine();
                string note = noteReader.ReadLine();

                // TODO probably clearer ways to validate but I honestly do not understand how validation works
                if (submittedText == null)
                {
                    if (responseText != null)
                    {
                        ModelState.AddModelError("ResponseText", "Unmatched Response without Submission");
                    }
                    if (isSolution != null)
                    {
                        ModelState.AddModelError("IsSolution", "Unmatched IsSolution without Submission");
                    }
                    if (note != null)
                    {
                        ModelState.AddModelError("Note", "Unmatched Note without Submission");
                    }

                    // we're done
                    break;
                }

                string submittedTextFormatted = ServerCore.DataModel.Response.FormatSubmission(submittedText);

                // Ensure that the submission text is unique for this puzzle.
                async Task<bool> isSubmittedTextUnique(string text)
                {
                    bool duplicateResponse = await(from Response r in _context.Responses
                                                   where r.PuzzleID == puzzleId &&
                                                   r.SubmittedText == text
                                                   select r).AnyAsync();

                    if (duplicateResponse)
                    {
                        ModelState.AddModelError("SubmittedText", "Submission text is not unique");
                    }

                    return !duplicateResponse;
                };

                if (!await isSubmittedTextUnique(submittedTextFormatted))
                {
                    break;
                }

                if (!submissions.Add(submittedTextFormatted))
                {
                    ModelState.AddModelError("SubmittedText", "Submission text is not unique");
                    break;
                }

                if (responseText == null)
                {
                    ModelState.AddModelError("SubmittedText", "Unmatched Submission without Response");
                    break;
                }

                isSolution = isSolution == null ? string.Empty : isSolution.ToLower();

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
                return await OnGetAsync(puzzleId);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { puzzleid = puzzleId });
        }
    }
}