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

namespace ServerCore.Pages.Puzzles
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class EditModel : EventSpecificPageModel
    {
        public EditModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public Puzzle Puzzle { get; set; }

        [BindProperty]
        public int NewAuthorID { get; set; }

        [BindProperty]
        public int NewPrerequisiteID { get; set; }

        [BindProperty]
        public int NewPrerequisiteOfID { get; set; }

        public List<PuzzleUser> PotentialAuthors { get; set; }

        public List<PuzzleUser> CurrentAuthors { get; set; }

        public List<Puzzle> PotentialPrerequisites { get; set; }

        public List<Puzzle> CurrentPrerequisites { get; set; }

        public List<Puzzle> PotentialPrerequisitesOf { get; set; }

        public List<Puzzle> CurrentPrerequisitesOf { get; set; }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound();
            }
            await PopulateUIAsync();

            return Page();
        }

        private async Task PopulateUIAsync()
        {
            IQueryable<PuzzleUser> currentAuthorsQ = _context.PuzzleAuthors.Where(m => m.Puzzle == Puzzle).Select(m => m.Author);
            IQueryable<PuzzleUser> potentialAuthorsQ = _context.EventAuthors.Where(m => m.Event == Event).Select(m => m.Author).Except(currentAuthorsQ);

            CurrentAuthors = await currentAuthorsQ.OrderBy(p => p.Name).ToListAsync();
            PotentialAuthors = await potentialAuthorsQ.OrderBy(p => p.Name).ToListAsync();

            IQueryable<Puzzle> allVisiblePuzzles;
            IQueryable<Puzzle> allVisiblePuzzlesAndGlobalPrerequisites;

            if (EventRole == EventRole.admin)
            {
                allVisiblePuzzles = allVisiblePuzzlesAndGlobalPrerequisites = _context.Puzzles.Where(m => m.Event == Event && m != Puzzle);
            }
            else
            {
                IQueryable<Puzzle> authorPuzzles = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser);
                Puzzle[] thisPuzzle = new Puzzle[] { Puzzle };

                allVisiblePuzzles = authorPuzzles.Except(thisPuzzle);
                allVisiblePuzzlesAndGlobalPrerequisites = authorPuzzles.Union(_context.Puzzles.Where(m => m.Event == Event && m.IsGloballyVisiblePrerequisite)).Except(thisPuzzle);
            }

            IQueryable<Puzzle> currentPrerequisitesQ = _context.Prerequisites.Where(m => m.Puzzle == Puzzle).Select(m => m.Prerequisite);
            IQueryable<Puzzle> potentialPrerequitesQ = allVisiblePuzzlesAndGlobalPrerequisites.Except(currentPrerequisitesQ);

            CurrentPrerequisites = await currentPrerequisitesQ.OrderBy(p => p.Name).ToListAsync();
            PotentialPrerequisites = await potentialPrerequitesQ.OrderBy(p => p.Name).ToListAsync();

            IQueryable<Puzzle> currentPrerequisitesOfQ = _context.Prerequisites.Where(m => m.Prerequisite == Puzzle).Select(m => m.Puzzle);
            IQueryable<Puzzle> potentialPrerequitesOfQ = allVisiblePuzzles.Except(currentPrerequisitesOfQ);

            CurrentPrerequisitesOf = await currentPrerequisitesOfQ.OrderBy(p => p.Name).ToListAsync();
            PotentialPrerequisitesOf = await potentialPrerequitesOfQ.OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Puzzle.Event");
            if (!ModelState.IsValid)
            {
                await PopulateUIAsync();
                return Page();
            }

            _context.Attach(Puzzle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PuzzleExists(Puzzle.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostAddAuthorAsync()
        {
            if (!(await _context.EventAuthors.Select(m => m.Author.ID == NewAuthorID && m.Event == Event).AnyAsync()))
            {
                return NotFound();
            }

            if (!(await _context.PuzzleAuthors.Select(m => m.PuzzleID == Puzzle.ID && m.AuthorID == NewAuthorID).AnyAsync()))
            {
                _context.PuzzleAuthors.Add(new PuzzleAuthors() { PuzzleID = Puzzle.ID, AuthorID = NewAuthorID });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddPrerequisiteAsync()
        {
            if (!PuzzleExists(NewPrerequisiteID))
            {
                return NotFound();
            }

            if (!PrerequisiteExists(Puzzle.ID, NewPrerequisiteID))
            {
                _context.Prerequisites.Add(new Prerequisites() { PuzzleID = Puzzle.ID, PrerequisiteID = NewPrerequisiteID });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddPrerequisiteOfAsync()
        {
            if (!PuzzleExists(NewPrerequisiteOfID))
            {
                return NotFound();
            }

            if (!PrerequisiteExists(NewPrerequisiteOfID, Puzzle.ID))
            {
                _context.Prerequisites.Add(new Prerequisites() { PuzzleID = NewPrerequisiteOfID, PrerequisiteID = Puzzle.ID });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetRemoveAuthorAsync(int puzzleId, int author)
        {
            PuzzleAuthors toRemove = await _context.PuzzleAuthors.Where(m => m.PuzzleID == puzzleId && m.AuthorID == author).FirstOrDefaultAsync();

            if (toRemove != null)
            {
                _context.PuzzleAuthors.Remove(toRemove);
                await _context.SaveChangesAsync();
            }

            // redirect without the prerequisite info to keep the URL clean
            return RedirectToPage(new { puzzleId });
        }

        public async Task<IActionResult> OnGetRemovePrerequisiteAsync(int puzzleId, int prerequisite)
        {
            Prerequisites toRemove = await _context.Prerequisites.Where(m => m.PuzzleID == puzzleId && m.PrerequisiteID == prerequisite).FirstOrDefaultAsync();

            if (toRemove != null)
            {
                _context.Prerequisites.Remove(toRemove);
                await _context.SaveChangesAsync();
            }

            // redirect without the prerequisite info to keep the URL clean
            return RedirectToPage(new { puzzleId });
        }

        public async Task<IActionResult> OnGetRemovePrerequisiteOfAsync(int puzzleId, int prerequisiteOf)
        {
            Prerequisites toRemove = await _context.Prerequisites.Where(m => m.PuzzleID == prerequisiteOf && m.PrerequisiteID == puzzleId).FirstOrDefaultAsync();

            if (toRemove != null)
            {
                _context.Prerequisites.Remove(toRemove);
                await _context.SaveChangesAsync();
            }

            // redirect without the prerequisite info to keep the URL clean
            return RedirectToPage(new { puzzleId });
        }

        private bool PuzzleExists(int puzzleId)
        {
            return _context.Puzzles.Any(e => e.ID == puzzleId);
        }

        private bool PrerequisiteExists(int puzzleId, int prerequisiteId)
        {
            return _context.Prerequisites.Any(pr => pr.Puzzle.ID == puzzleId && pr.Prerequisite.ID == prerequisiteId);
        }
    }
}
