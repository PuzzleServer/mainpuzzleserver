using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsEventAdmin")]
    public class ImportModel : EventSpecificPageModel
    {
        public ImportModel(PuzzleServerContext context, UserManager<IdentityUser> userManager) : base(context, userManager)
        {
        }

        public IList<Event> Events { get; set; }

        [BindProperty]
        public int ImportEventID { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Events = await _context.EventAdmins.Where(ea => ea.Admin == LoggedInUser && ea.Event != Event).Select((ea) => ea.Event).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostImportAsync()
        {
            // the BindProperty only binds the event ID, let's get the rest
            if (await _context.Events.Where((e) => e.ID == ImportEventID).FirstOrDefaultAsync() == null)
            {
                return NotFound();
            }

            // verify that we're an admin of the import event. current event administratorship is already validated.
            if (!await _context.EventAdmins.Where(ea => ea.Event.ID == ImportEventID && ea.Admin == LoggedInUser).AnyAsync())
            {
                return Forbid();
            }

            var sourceEventAuthors = await _context.EventAuthors.Where((a) => a.Event.ID == ImportEventID).ToListAsync();
            var sourcePuzzles = await _context.Puzzles.Where((p) => p.Event.ID == ImportEventID).ToListAsync();

            // TODO: replace this with a checkbox and sufficient danger warnings about duplicate titles
            bool deletePuzzleIfPresent = true;

            using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                // Step 1: Make sure all authors exist
                foreach (var sourceEventAuthor in sourceEventAuthors)
                {
                    var destEventAuthor = await _context.EventAuthors.Where((e) => e.Event == Event && e.Author == sourceEventAuthor.Author).FirstOrDefaultAsync();

                    if (destEventAuthor == null)
                    {
                        destEventAuthor = new EventAuthors(sourceEventAuthor);
                        destEventAuthor.Event = Event;
                        _context.EventAuthors.Add(destEventAuthor);
                    }
                }

                // Step 2: Make sure all puzzles exist
                Dictionary<int, Puzzle> puzzleCloneMap = new Dictionary<int, Puzzle>();

                foreach (var sourcePuzzle in sourcePuzzles)
                {
                    // delete the puzzle if it exists
                    if (deletePuzzleIfPresent)
                    {
                        foreach (Puzzle p in _context.Puzzles.Where((p) => p.Event == Event && p.Name == sourcePuzzle.Name))
                        {
                            await PuzzleHelper.DeletePuzzleAsync(_context, p);
                        }
                    }

                    var destPuzzle = new Puzzle(sourcePuzzle);
                    destPuzzle.Event = Event;

                    puzzleCloneMap[sourcePuzzle.ID] = destPuzzle;
                    _context.Puzzles.Add(destPuzzle);
                }

                // Step 3: Save so that all our new objects have valid IDs
                await _context.SaveChangesAsync();

                // Step 4: Ancillary tables referring to puzzles
                foreach (var sourcePuzzle in sourcePuzzles)
                {
                    // PuzzleAuthors
                    foreach (PuzzleAuthors sourcePuzzleAuthor in _context.PuzzleAuthors.Where((p) => p.Puzzle == sourcePuzzle))
                    {
                        var destPuzzleAuthor = new PuzzleAuthors(sourcePuzzleAuthor);
                        destPuzzleAuthor.Puzzle = puzzleCloneMap[sourcePuzzleAuthor.Puzzle.ID];
                        _context.PuzzleAuthors.Add(destPuzzleAuthor);
                    }

                    // Responses
                    foreach (Response sourceResponse in _context.Responses.Where((r) => r.Puzzle == sourcePuzzle))
                    {
                        var destResponse = new Response(sourceResponse);
                        destResponse.Puzzle = puzzleCloneMap[sourceResponse.Puzzle.ID];
                        _context.Responses.Add(destResponse);
                    }

                    // Prerequisites
                    foreach (Prerequisites sourcePrerequisite in _context.Prerequisites.Where((r) => r.Puzzle == sourcePuzzle))
                    {
                        var destPrerequisite = new Prerequisites(sourcePrerequisite);
                        destPrerequisite.Puzzle = puzzleCloneMap[sourcePrerequisite.Puzzle.ID];
                        destPrerequisite.Prerequisite = puzzleCloneMap[sourcePrerequisite.Prerequisite.ID];
                        _context.Prerequisites.Add(destPrerequisite);
                    }

                    // Hints
                    foreach (Hint sourceHint in _context.Hints.Where((h) => h.Puzzle == sourcePuzzle))
                    {
                        var destHint = new Hint(sourceHint);
                        destHint.Puzzle = puzzleCloneMap[sourceHint.Puzzle.ID];
                        _context.Hints.Add(destHint);

                        foreach (Team team in _context.Teams.Where(t => t.Event == Event))
                        {
                            _context.HintStatePerTeam.Add(new HintStatePerTeam() { Hint = destHint, TeamID = team.ID });
                        }
                    }

                    // PuzzleStatePerTeam
                    foreach (Team team in _context.Teams.Where(t => t.Event == Event))
                    {
                        int newPuzzleId = puzzleCloneMap[sourcePuzzle.ID].ID;
                        bool hasPuzzleStatePerTeam = await (from pspt in _context.PuzzleStatePerTeam
                                                            where pspt.PuzzleID == newPuzzleId &&
                                                            pspt.TeamID == team.ID
                                                            select pspt).AnyAsync();
                        if (!hasPuzzleStatePerTeam)
                        {
                            PuzzleStatePerTeam newPspt = new PuzzleStatePerTeam() { TeamID = team.ID, PuzzleID = newPuzzleId };
                            _context.PuzzleStatePerTeam.Add(newPspt);
                        }
                    }

                    // ContentFiles
                    foreach (ContentFile contentFile in _context.ContentFiles.Where((c) => c.Puzzle == sourcePuzzle))
                    {
                        ContentFile newFile = new ContentFile(contentFile);
                        newFile.Event = Event;
                        newFile.Puzzle = puzzleCloneMap[contentFile.Puzzle.ID];
                        newFile.Url = await FileManager.CloneBlobAsync(contentFile.ShortName, Event.ID, contentFile.Url);
                        _context.ContentFiles.Add(newFile);
                    }
                }

                // Step 5: Final save and commit
                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            return RedirectToPage("./Index");
        }
    }
}
