using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
    /// Page for managing all files in bulk
    /// </summary>
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class BulkFilesModel : EventSpecificPageModel
    {
        public BulkFilesModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public List<string> FoldersCopied { get; set; } = new List<string>();

        [BindProperty]
        public List<string> FilesIgnored { get; set; } = new List<string>();

        [BindProperty]
        public List<IFormFile> NewBulkFiles { get; set; }

        /// <summary>
        /// Handler for listing files
        /// </summary>
        public IActionResult OnGet(int puzzleId)
        {
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

            if (NewBulkFiles != null)
            {
                foreach (IFormFile resourceFile in NewBulkFiles)
                {
                    await UploadFileAsync(resourceFile);
                }
            }

            return Page();
        }

        /// <summary>
        /// Helper for taking an uploaded form file, uploading it, and tracking it in the database
        /// </summary>
        private async Task UploadFileAsync(IFormFile uploadedFile)
        {
            if (Path.GetExtension(uploadedFile.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                await UploadZipAsync(uploadedFile);
            }
            else
            {
                FilesIgnored.Add(uploadedFile.FileName);
            }
        }

        private class DirData
        {
            public string RawDirectory;
            public int PuzzleID;
            public ContentFileType FileType;
            public Dictionary<string, Stream> Files = new Dictionary<string, Stream>();
        }

        /// <summary>
        /// Extracts the contents of a zip file and uploads the contents into a single directory
        /// </summary>
        private async Task UploadZipAsync(IFormFile uploadedFile)
        {
            Uri fileStoragePrefix = new Uri(FileManager.GetFileStoragePrefix(Event.ID, "") + "/", UriKind.Absolute);
            List<int> authorPuzzles = (EventRole == EventRole.admin) ? null : await _context.PuzzleAuthors.Where(pa => pa.Author.ID == LoggedInUser.ID).Select(pa => pa.PuzzleID).ToListAsync();

            Dictionary<string, DirData> dirs = new Dictionary<string, DirData>();

            //
            // Build a mapping table of all existing content
            //
            Dictionary<string, ContentFile> existingContent = await (from contentFile in this._context.ContentFiles
                                         where contentFile.Event == this.Event
                                         select contentFile).ToDictionaryAsync(c => c.ShortName);

            foreach (ContentFile contentFile in existingContent.Values)
            {
                string originalDirectory = Path.GetDirectoryName(contentFile.ShortName);
                
                if (!dirs.ContainsKey(originalDirectory))
                {
                    Uri uriMinusPrefix = fileStoragePrefix.MakeRelativeUri(contentFile.Url);
                    string rawDirectory = Uri.UnescapeDataString(uriMinusPrefix.OriginalString.Split('/')[0]);
                    dirs[originalDirectory] = (authorPuzzles != null && !authorPuzzles.Contains(contentFile.PuzzleID)) ? null : new DirData() { RawDirectory = rawDirectory, PuzzleID = contentFile.PuzzleID, FileType = contentFile.FileType, Files = new Dictionary<string, Stream>() };
                }
            }

            //
            // Walk through all files in the zip, strip puzzles/ and solutions/ directory prefixes if present, and map to existing structure
            //
            ZipArchive archive = new ZipArchive(uploadedFile.OpenReadStream(), ZipArchiveMode.Read);
            foreach (ZipArchiveEntry entry in archive.Entries)
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
                string directory = fileName.Split('/')[0];
                if (directory == "puzzles" || directory == "solutions")
                {
                    fileName = fileName.Substring(directory.Length + 1);
                    directory = fileName.Split('/')[0];
                }

                if (dirs.ContainsKey(directory) && dirs[directory] != null)
                {
                    dirs[directory].Files[fileName] = entry.Open();
                }
                else
                {
                    FilesIgnored.Add(entry.FullName);
                }
            }

            //
            // Now upload files and add missing ContentFiles, one directory at a time
            //
            foreach (var pair in dirs)
            {
                Dictionary<string, Uri> fileUrls = await FileManager.UploadBlobsAsync(pair.Value.Files, Event.ID, pair.Value.RawDirectory);
                bool addedAny = false;

                foreach (string shortName in fileUrls.Keys)
                {
                    if (!existingContent.ContainsKey(shortName))
                    {
                        ContentFile file = new ContentFile()
                        {
                            ShortName = shortName,
                            PuzzleID = pair.Value.PuzzleID,
                            Event = Event,
                            FileType = pair.Value.FileType,
                            Url = fileUrls[shortName],
                        };
                        _context.ContentFiles.Add(file);
                        addedAny = true;
                    }
                }

                if (addedAny)
                {
                    await _context.SaveChangesAsync();
                }

                FoldersCopied.Add(pair.Key);
            }
        }
    }
}
