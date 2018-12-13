using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Pages.Events
{
    public class ImportModel : PageModel
    {
        private readonly PuzzleServerContext _context;

        public ImportModel(PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Event Event { get; set; }

        public IList<Event> Events { get; set; }

        [BindProperty]
        public int ImportEventID { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Event = await _context.Events.SingleOrDefaultAsync(m => m.ID == id);

            if (Event == null)
            {
                return NotFound();
            }

            Events = await _context.Events.Where(e => e != Event).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostImportAsync()
        {
            // the BindProperty only binds the event ID, let's get the rest
            Event = await _context.Events.Where((e) => e.ID == Event.ID).FirstOrDefaultAsync();

            if (Event == null || await _context.Events.Where((e) => e.ID == ImportEventID).FirstOrDefaultAsync() == null)
            {
                return NotFound();
            }

            // TODO: validate that you are an admin of both events!!!

            var sourceEventAuthors = await _context.EventAuthors.Where((a) => a.Event.ID == ImportEventID).ToListAsync();
            var sourcePuzzles = await _context.Puzzles.Where((p) => p.Event.ID == ImportEventID).ToListAsync();

            // TODO: replace this with a checkbox and sufficient danger warnings about duplicate titles
            bool deletePuzzleIfPresent = true;

            using (var transaction = _context.Database.BeginTransaction())
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
                            // TODO why do I need a null check here? Does that mean something is improperly wired up?
                            if (p.Contents != null)
                            {
                                foreach (ContentFile content in p.Contents)
                                {
                                    await FileManager.DeleteBlobAsync(content.Url);
                                }
                            }

                            _context.Puzzles.Remove(p);
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
                    }

                    // TODO: ContentFiles
                }

                // Step 5: Final save and commit
                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            return RedirectToPage("./Index");
        }
    }
}
