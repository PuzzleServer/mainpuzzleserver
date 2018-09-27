using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    public class EditModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public EditModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Puzzle Puzzle { get; set; }

        [BindProperty]
        public IFormFile PuzzleFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Puzzle = await _context.Puzzles.Where(m => m.ID == id).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Puzzle).State = EntityState.Modified;

            if (PuzzleFile != null)
            {
                ContentFile file = new ContentFile();
                string fileName = WebUtility.UrlEncode(PuzzleFile.FileName);

                file.ShortName = fileName;
                file.Puzzle = Puzzle;
                file.Event = Event;
                file.FileType = ContentFileType.Puzzle;

                // Remove previous puzzle files
                var oldFiles = from oldFile in _context.ContentFiles
                               where oldFile.Puzzle == Puzzle &&
                               oldFile.FileType == ContentFileType.Puzzle
                               select oldFile;
                foreach(ContentFile oldFile in oldFiles)
                {
                    await FileManager.DeleteBlobAsync(oldFile.Url);
                    _context.ContentFiles.Remove(oldFile);
                }

                file.Url = await FileManager.UploadBlobAsync(fileName, Event.ID, PuzzleFile.OpenReadStream());

                _context.ContentFiles.Add(file);
            }

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

        private bool PuzzleExists(int id)
        {
            return _context.Puzzles.Any(e => e.ID == id);
        }
    }
}
