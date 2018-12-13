using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class EditModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public EditModel(PuzzleServerContext context)
        {
            _context = context;
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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Puzzle = await _context.Puzzles.Where(m => m.ID == id).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound();
            }

            IQueryable<PuzzleUser> currentAuthorsQ = _context.PuzzleAuthors.Where(m => m.Puzzle == Puzzle).Select(m => m.Author);
            IQueryable<PuzzleUser> potentialAuthorsQ = _context.EventAuthors.Where(m => m.Event == Event).Select(m => m.Author).Except(currentAuthorsQ);

            CurrentAuthors = await currentAuthorsQ.OrderBy(p => p.Name).ToListAsync();
            PotentialAuthors = await potentialAuthorsQ.OrderBy(p => p.Name).ToListAsync();

            IQueryable<Puzzle> currentPrerequisitesQ = _context.Prerequisites.Where(m => m.Puzzle == Puzzle).Select(m => m.Prerequisite);
            IQueryable<Puzzle> potentialPrerequitesQ = _context.Puzzles.Where(m => m.Event == Event && m != Puzzle).Except(currentPrerequisitesQ);

            CurrentPrerequisites = await currentPrerequisitesQ.OrderBy(p => p.Name).ToListAsync();
            PotentialPrerequisites = await potentialPrerequitesQ.OrderBy(p => p.Name).ToListAsync();

            IQueryable<Puzzle> currentPrerequisitesOfQ = _context.Prerequisites.Where(m => m.Prerequisite == Puzzle).Select(m => m.Puzzle);
            IQueryable<Puzzle> potentialPrerequitesOfQ = _context.Puzzles.Where(m => m.Event == Event && m != Puzzle).Except(currentPrerequisitesOfQ);

            CurrentPrerequisitesOf = await currentPrerequisitesOfQ.OrderBy(p => p.Name).ToListAsync();
            PotentialPrerequisitesOf = await potentialPrerequitesOfQ.OrderBy(p => p.Name).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
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

        public async Task<IActionResult> OnGetRemoveAuthorAsync(int id, int author)
        {
            PuzzleAuthors toRemove = await _context.PuzzleAuthors.Where(m => m.PuzzleID == id && m.AuthorID == author).FirstOrDefaultAsync();

            if (toRemove != null)
            {
                _context.PuzzleAuthors.Remove(toRemove);
                await _context.SaveChangesAsync();
            }

            // redirect without the prerequisite info to keep the URL clean
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnGetRemovePrerequisiteAsync(int id, int prerequisite)
        {
            Prerequisites toRemove = await _context.Prerequisites.Where(m => m.PuzzleID == id && m.PrerequisiteID == prerequisite).FirstOrDefaultAsync();

            if (toRemove != null)
            {
                _context.Prerequisites.Remove(toRemove);
                await _context.SaveChangesAsync();
            }

            // redirect without the prerequisite info to keep the URL clean
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnGetRemovePrerequisiteOfAsync(int id, int prerequisiteOf)
        {
            Prerequisites toRemove = await _context.Prerequisites.Where(m => m.PuzzleID == prerequisiteOf && m.PrerequisiteID == id).FirstOrDefaultAsync();

            if (toRemove != null)
            {
                _context.Prerequisites.Remove(toRemove);
                await _context.SaveChangesAsync();
            }

            // redirect without the prerequisite info to keep the URL clean
            return RedirectToPage(new { id });
        }

        private bool PuzzleExists(int id)
        {
            return _context.Puzzles.Any(e => e.ID == id);
        }

        private bool PrerequisiteExists(int puzzleId, int prerequisiteId)
        {
            return _context.Prerequisites.Any(pr => pr.Puzzle.ID == puzzleId && pr.Prerequisite.ID == prerequisiteId);
        }
    }
}
