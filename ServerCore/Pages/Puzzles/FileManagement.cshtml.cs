using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public Puzzle Puzzle { get; set; }

        [BindProperty]
        public IFormFile PuzzleFile { get; set; }

        [BindProperty]
        public IFormFile AnswerFile { get; set; }

        [BindProperty]
        public List<IFormFile> PuzzleMaterialFiles { get; set; }

        [BindProperty]
        public List<IFormFile> SolveTokenFiles { get; set; }

        [BindProperty]
        public bool ExpandZipFiles { get; set; }

        [BindProperty]
        public List<int> SelectedFiles { get; set; }

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

            try
            {
                await DeleteFileAsync(fileToDelete);
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

        private async Task DeleteFileAsync(ContentFile fileToDelete)
        {
            await FileManager.DeleteBlobAsync(fileToDelete.Url);
            Puzzle.Contents.Remove(fileToDelete);
            Puzzle.PuzzleVersion = Puzzle.PuzzleVersion + 1;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes files selected by checkboxes
        /// </summary>
        public async Task<IActionResult> OnPostDeleteSelectedAsync(int puzzleId)
        {
            Puzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.ID == puzzleId);

            if (Puzzle == null)
            {
                return NotFound();
            }

            var filesToDelete = await (from file in _context.ContentFiles
                                       where file.PuzzleID == puzzleId &&
                                       SelectedFiles.Contains(file.ID)
                                       select file).ToListAsync();

            foreach(ContentFile fileToDelete in filesToDelete)
            {
                try
                {
                    await DeleteFileAsync(fileToDelete);
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
            }

            return RedirectToPage("FileManagement");
        }

        /// <summary>
        /// Deletes all files from this puzzle
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAllAsync(int puzzleId)
        {
            Puzzle = await _context.Puzzles.FirstOrDefaultAsync(m => m.ID == puzzleId);

            if (Puzzle == null)
            {
                return NotFound();
            }

            var filesToDelete = await (from file in _context.ContentFiles
                                       where file.PuzzleID == puzzleId
                                       select file).ToListAsync();

            foreach (ContentFile fileToDelete in filesToDelete)
            {
                try
                {
                    await DeleteFileAsync(fileToDelete);
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
        /// Makes a clean link to the ContentFile based on ShortName, that is specific to this event.
        /// </summary>
        /// <param name="file">The ContentFile</param>
        /// <returns>a link that explicitly includes the eventId</returns>
        public string LinkFromShortName(ContentFile file)
        {
            return this.HttpContext.Request.Scheme + "://" + this.HttpContext.Request.Host + "/" + this.Event.ID + "/Files/" + file.ShortName;
        }

        /// <summary>
        /// Makes a clean link to the ContentFile based on ShortName, that is not specific to this event.
        /// </summary>
        /// <param name="file">The ContentFile</param>
        /// <returns>a link that replaces the explicit eventId with {eventId}, best used for Puzzle.CustomURL and Puzzle.CustomSolutionURL.</returns>
        public string EventAgnosticLinkFromShortName(ContentFile file)
        {
            // lack of translation of {eventId} is very intentional - this is translated at runtime and makes the field value portable
            return this.HttpContext.Request.Scheme + "://" + this.HttpContext.Request.Host + "/{eventId}/Files/" + file.ShortName;
        }

        /// <summary>
        /// Promotes files named "index.html" to the CustomURL or CustomSolutionURL properties of the puzzle.
        /// </summary>
        /// <param name="file">The file being uploaded</param>
        private void UpdateCustomURLs(ContentFile file)
        {
            // Skip this if:
            // - the file is not a material or solution file
            // - the appropriate Custom[Solution]URL is non-blank
            if (!(file.FileType == ContentFileType.PuzzleMaterial || file.FileType == ContentFileType.SolveToken) || 
                (file.FileType == ContentFileType.PuzzleMaterial && !string.IsNullOrEmpty(Puzzle.CustomURL)) ||
                (file.FileType == ContentFileType.SolveToken && !string.IsNullOrEmpty(Puzzle.CustomSolutionURL)))
            {
                return;
            }

            string fileNameLower = Path.GetFileName(file.ShortName).ToLower();
            if (fileNameLower == "index.html")
            {
                string filePath = this.EventAgnosticLinkFromShortName(file);
                if (file.FileType == ContentFileType.PuzzleMaterial)
                {
                    Puzzle.CustomURL = filePath;
                }
                else
                {
                    Puzzle.CustomSolutionURL = filePath;
                }
            }
            else if (fileNameLower.EndsWith("_client.html") && file.FileType == ContentFileType.PuzzleMaterial)
            {
                Puzzle.CustomURL = this.HttpContext.Request.Scheme + "://" + this.HttpContext.Request.Host + "/{eventId}/{puzzleId}/api/Sync/client/?eventId={eventId}";
            }
        }

        /// <summary>
        /// Helper for taking an uploaded form file, uploading it, and tracking it in the database
        /// </summary>
        private async Task UploadFileAsync(IFormFile uploadedFile, ContentFileType fileType)
        {
            if ((fileType == ContentFileType.PuzzleMaterial || fileType == ContentFileType.SolveToken) &&
                Path.GetExtension(uploadedFile.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase) && ExpandZipFiles)
            {
                await UploadZipAsync(uploadedFile, fileType);
                return;
            }

            ContentFile file = new ContentFile();
            string fileName = uploadedFile.FileName;
            file.ShortName = fileName;
            file.Puzzle = Puzzle;
            file.Event = Event;
            file.FileType = fileType;

            file.Url = await FileManager.UploadBlobAsync(fileName, Event.ID, uploadedFile.OpenReadStream());

            this.UpdateCustomURLs(file);
            _context.ContentFiles.Add(file);
        }

        /// <summary>
        /// Extracts the contents of a zip file and uploads the contents into a single directory
        /// </summary>
        private async Task UploadZipAsync(IFormFile uploadedFile, ContentFileType fileType)
        {
            ZipArchive archive = new ZipArchive(uploadedFile.OpenReadStream(), ZipArchiveMode.Read);
            Dictionary<string, Stream> contents = new Dictionary<string, Stream>();
            foreach(ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith("/"))
                {
                    continue;
                }

                if (entry.FullName.StartsWith("/") || entry.FullName.StartsWith("\\") || entry.FullName.Contains(".."))
                {
                    throw new ArgumentException();
                }

                string fileName = entry.FullName;
                contents[fileName] = entry.Open();
            }

            Dictionary<string, Uri> fileUrls = await FileManager.UploadBlobsAsync(contents, Event.ID);
            foreach(KeyValuePair<string, Uri> fileUrl in fileUrls)
            {
                ContentFile file = new ContentFile()
                {
                    ShortName = fileUrl.Key,
                    Puzzle = Puzzle,
                    Event = Event,
                    FileType = fileType,
                    Url = fileUrl.Value,
                };
                this.UpdateCustomURLs(file);
                _context.ContentFiles.Add(file);
            }
        }
    }
}
