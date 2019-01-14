using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult OnGet(int puzzleId)
        {
            PuzzleId = puzzleId;
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
                        throw new ArgumentException("Unmatched response without submission");
                    }
                    if (isSolution != null)
                    {
                        throw new ArgumentException("Unmatched IsSolution without submission");
                    }
                    if (note != null)
                    {
                        throw new ArgumentException("Unmatched note without submission");
                    }

                    // we're done
                    break;
                }

                if (responseText == null)
                {
                    throw new ArgumentException("Unmatched submission without response");
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

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { puzzleid = puzzleId });
        }
    }
}