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

namespace ServerCore.Pages.Hints
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class CreateBulkModel : EventSpecificPageModel
    {
        public CreateBulkModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public bool DeleteExisting { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public string HintContent { get; set; }

        [BindProperty]
        public string Cost { get; set; }

        [BindProperty]
        public string DisplayOrder { get; set; }

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
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            if (Puzzle == null || !ModelState.IsValid)
            {
                return Page();
            }

            if (DeleteExisting)
            {
                Hint[] hintsToRemove = await (from Hint h in _context.Hints
                                                      where h.PuzzleID == puzzleId
                                                      select h).ToArrayAsync();
                _context.Hints.RemoveRange(hintsToRemove);
            }

            using StringReader descriptionReader = new StringReader(Description ?? string.Empty);
            using StringReader contentReader = new StringReader(HintContent ?? string.Empty);
            using StringReader costReader = new StringReader(Cost ?? string.Empty);
            using StringReader displayOrderReader = new StringReader(DisplayOrder ?? string.Empty);

            var teams = from team in _context.Teams
                        where team.Event == Event
                        select team;

            while (true)
            {
                string description = descriptionReader.ReadLine();
                string content = contentReader.ReadLine();
                string cost = costReader.ReadLine();
                string displayOrder = displayOrderReader.ReadLine();

                // TODO probably clearer ways to validate but I honestly do not understand how validation works
                if (description == null)
                {
                    if (content != null)
                    {
                        ModelState.AddModelError("HintContent", "Unmatched Content without Description");
                    }
                    if (cost != null)
                    {
                        ModelState.AddModelError("Cost", "Unmatched Cost without Description");
                    }
                    if (displayOrder != null)
                    {
                        ModelState.AddModelError("DisplayOrder", "Unmatched DisplayOrder without Description");
                    }

                    // we're done
                    break;
                }

                int costInt;
                int displayOrderInt;

                if (content == null)
                {
                    ModelState.AddModelError("HintContent", $"Content must be present");
                }

                // int checks also handle the null case
                if (!int.TryParse(cost, out costInt))
                {
                    ModelState.AddModelError("Cost", $"Cost of '{cost}' must be an integer");
                    break;
                }

                if (!int.TryParse(displayOrder, out displayOrderInt))
                {
                    ModelState.AddModelError("DisplayOrder", $"DisplayOrder of '{displayOrder}' must be an integer");
                    break;
                }

                Hint hint = new Hint()
                {
                    PuzzleID = puzzleId,
                    Description = description,
                    Content = content,
                    Cost = costInt,
                    DisplayOrder = displayOrderInt
                };

                _context.Hints.Add(hint);

                if (!Puzzle.IsForSinglePlayer)
                {
                    foreach (Team team in teams)
                    {
                        _context.HintStatePerTeam.Add(new HintStatePerTeam() { Hint = hint, Team = team });
                    }
                }
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