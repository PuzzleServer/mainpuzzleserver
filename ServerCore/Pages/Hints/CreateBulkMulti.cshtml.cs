using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Hints
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
        public string Description { get; set; }

        [BindProperty]
        public string HintContent { get; set; }

        [BindProperty]
        public string Cost { get; set; }

        [BindProperty]
        public string DisplayOrder { get; set; }

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
            using StringReader descriptionReader = new StringReader(Description ?? string.Empty);
            using StringReader contentReader = new StringReader(HintContent ?? string.Empty);
            using StringReader costReader = new StringReader(Cost ?? string.Empty);
            using StringReader displayOrderReader = new StringReader(DisplayOrder ?? string.Empty);

            Dictionary<string, int> puzzleTitleLookup = new Dictionary<string, int>();

            var teams = from team in _context.Teams
                        where team.Event == Event
                        select team;

            var players = from player in _context.PlayerInEvent
                          where player.Event == Event
                          select player;

            while (true)
            {
                string puzzleName = puzzleNameReader.ReadLine();
                string description = descriptionReader.ReadLine();
                string content = contentReader.ReadLine();
                string cost = costReader.ReadLine();
                string displayOrder = displayOrderReader.ReadLine();

                int puzzleId = -1;

                if (puzzleName != null && !puzzleTitleLookup.TryGetValue(puzzleName, out puzzleId))
                {
                    Puzzle puzzle = await (from Puzzle p in _context.Puzzles
                                           where p.Name == puzzleName && p.EventID == this.Event.ID
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

                    if (DeleteExisting)
                    {
                        Hint[] hintsToRemove = await (from Hint h in _context.Hints
                                                              where h.PuzzleID == puzzleId
                                                              select h).ToArrayAsync();
                        _context.Hints.RemoveRange(hintsToRemove);
                    }
                }

                // TODO probably clearer ways to validate but I honestly do not understand how validation works
                if (puzzleName == null)
                {
                    if (description != null)
                    {
                        ModelState.AddModelError("Description", "Unmatched Description without Puzzle");
                    }
                    if (content != null)
                    {
                        ModelState.AddModelError("HintContent", "Unmatched Content without Puzzle");
                    }
                    if (cost != null)
                    {
                        ModelState.AddModelError("Cost", "Unmatched Cost without Puzzle");
                    }
                    if (displayOrder != null)
                    {
                        ModelState.AddModelError("DisplayOrder", "Unmatched DisplayOrder without Puzzle");
                    }

                    // we're done
                    break;
                }

                int costInt;
                int displayOrderInt;

                if (description == null)
                {
                    ModelState.AddModelError("Description", $"Description must be present");
                }

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

                if (puzzleId == -1)
                {
                    throw new Exception($"Bug in puzzleId lookup for {puzzleName}");
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
                if (hint.Puzzle.IsForSinglePlayer)
                {
                    foreach (PlayerInEvent player in players)
                    {
                        _context.SinglePlayerPuzzleHintStatePerPlayer.Add(
                            new SinglePlayerPuzzleHintStatePerPlayer() 
                            { 
                                Hint = hint, 
                                PlayerID = player.PlayerId
                            });
                    }
                }
                else
                {
                    foreach (Team team in teams)
                    {
                        _context.HintStatePerTeam.Add(new HintStatePerTeam() { Hint = hint, Team = team });
                    }
                }
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