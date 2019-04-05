using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    /// <summary>
    /// Page for managing the files associated with a puzzle
    /// </summary>
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class FileManagementModel : EventSpecificPageModel
    {
        public FileManagementModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        //[BindProperty]
        public Puzzle Puzzle { get; set; }

        [BindProperty]
        public IFormFile PuzzleFile { get; set; }

        [BindProperty]
        public IFormFile AnswerFile { get; set; }

        [BindProperty]
        public List<IFormFile> PuzzleMaterialFiles { get; set; }

        [BindProperty]
        public List<IFormFile> SolveTokenFiles { get; set; }

        /// <summary>
        /// Handler for listing files
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            Puzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.ID == puzzleId);

            if (Puzzle == null)
            {
                return NotFound();
            }

            return Page();
        }

        /// <summary>
        /// Handler for uploading files
        /// </summary>
        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Puzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.ID == puzzleId);

            if (Puzzle == null)
            {
                return NotFound();
            }

            bool changeMade = false;

            if (PuzzleFile != null)
            {
                // Remove previous puzzle files
                var oldFiles = from oldFile in _context.ContentFiles
                               where oldFile.Puzzle == Puzzle &&
                               oldFile.FileType == ContentFileType.Puzzle
                               select oldFile;
                foreach (ContentFile oldFile in oldFiles)
                {
                    await FileManager.DeleteBlobAsync(oldFile.Url);
                    _context.ContentFiles.Remove(oldFile);
                }
                await UploadFileAsync(PuzzleFile, ContentFileType.Puzzle);
                changeMade = true;
            }

            if (AnswerFile != null)
            {
                // Remove previous answer files
                var oldFiles = from oldFile in _context.ContentFiles
                               where oldFile.Puzzle == Puzzle &&
                               oldFile.FileType == ContentFileType.Answer
                               select oldFile;
                foreach (ContentFile oldFile in oldFiles)
                {
                    await FileManager.DeleteBlobAsync(oldFile.Url);
                    _context.ContentFiles.Remove(oldFile);
                }
                await UploadFileAsync(AnswerFile, ContentFileType.Answer);
                changeMade = true;
            }

            if (PuzzleMaterialFiles != null)
            {
                foreach (IFormFile materialFile in PuzzleMaterialFiles)
                {
                    await UploadFileAsync(materialFile, ContentFileType.PuzzleMaterial);
                    changeMade = true;
                }
            }

            if (SolveTokenFiles != null)
            {
                foreach (IFormFile solveToken in SolveTokenFiles)
                {
                    await UploadFileAsync(solveToken, ContentFileType.SolveToken);
                    changeMade = true;
                }
            }

            if (changeMade)
            {
                Puzzle.PuzzleVersion = Puzzle.PuzzleVersion + 1;
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

            return RedirectToPage("FileManagement");
        }

        /// <summary>
        /// Handler for deleting an uploaded file
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(int puzzleId, int fileId)
        {
            Puzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.ID == puzzleId);

            if (Puzzle == null)
            {
                return NotFound();
            }

            ContentFile fileToDelete = (from file in Puzzle.Contents
                                        where file.ID == fileId
                                        select file).SingleOrDefault();

            if (fileToDelete == null)
            {
                return NotFound();
            }

            await FileManager.DeleteBlobAsync(fileToDelete.Url);
            Puzzle.Contents.Remove(fileToDelete);
            Puzzle.PuzzleVersion = Puzzle.PuzzleVersion + 1;

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

            return RedirectToPage("FileManagement");
        }

        /// <summary>
        /// Returns true if a puzzle exists with a given id
        /// </summary>
        private bool PuzzleExists(int puzzleId)
        {
            return _context.Puzzles.Any(e => e.ID == puzzleId);
        }

        /// <summary>
        /// Helper for taking an uploaded form file, uploading it, and tracking it in the database
        /// </summary>
        private async Task UploadFileAsync(IFormFile uploadedFile, ContentFileType fileType)
        {
            ContentFile file = new ContentFile();
            string fileName = WebUtility.UrlEncode(Path.GetFileName(uploadedFile.FileName));

            file.ShortName = fileName;
            file.Puzzle = Puzzle;
            file.Event = Event;
            file.FileType = fileType;

            file.Url = await FileManager.UploadBlobAsync(fileName, Event.ID, uploadedFile.OpenReadStream());

            _context.ContentFiles.Add(file);
        }
    }
}
