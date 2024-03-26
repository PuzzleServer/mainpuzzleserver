using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Policy;
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
    public class ResourceManagementModel : EventSpecificPageModel
    {
        public ResourceManagementModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public List<IFormFile> NewResourceFiles { get; set; }

        [BindProperty]
        public bool ExpandZipFiles { get; set; }

        [BindProperty]
        public List<Uri> SelectedFiles { get; set; }

        public List<DirectoryFileResult> Resources { get; private set; }

        const string SharedResourceDirectoryName = "resources";

        /// <summary>
        /// Handler for listing files
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            this.Resources = await FileManager.GetDirectoryContents(this.Event.ID, SharedResourceDirectoryName);
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

            if (NewResourceFiles != null)
            {
                foreach (IFormFile resourceFile in NewResourceFiles)
                {
                    await UploadFileAsync(resourceFile);
                }
            }

            return RedirectToPage("ResourceManagement");
        }

        /// <summary>
        /// Handler for deleting an uploaded file
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(Uri url)
        {
            await FileManager.DeleteBlobAsync(url);

            return RedirectToPage("ResourceManagement");
        }

        /// <summary>
        /// Deletes files selected by checkboxes
        /// </summary>
        public async Task<IActionResult> OnPostDeleteSelectedAsync()
        {
            foreach (Uri resource in SelectedFiles)
            {
                await FileManager.DeleteBlobAsync(resource);
            }

            return RedirectToPage("ResourceManagement");
        }

        /// <summary>
        /// Deletes all files from this puzzle
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAllAsync()
        {
            foreach (var file in await FileManager.GetDirectoryContents(Event.ID, SharedResourceDirectoryName))
            {
                await FileManager.DeleteBlobAsync(file.Uri);
            }

            return RedirectToPage("ResourceManagement");
        }

        /// <summary>
        /// Helper for taking an uploaded form file, uploading it, and tracking it in the database
        /// </summary>
        private async Task UploadFileAsync(IFormFile uploadedFile)
        {
            if (Path.GetExtension(uploadedFile.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase) && ExpandZipFiles)
            {
                await UploadZipAsync(uploadedFile);
                return;
            }

            await FileManager.UploadBlobAsync(uploadedFile.FileName, Event.ID, uploadedFile.OpenReadStream(), SharedResourceDirectoryName);
        }

        /// <summary>
        /// Extracts the contents of a zip file and uploads the contents into a single directory
        /// </summary>
        private async Task UploadZipAsync(IFormFile uploadedFile)
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

            Dictionary<string, Uri> fileUrls = await FileManager.UploadBlobsAsync(contents, Event.ID, SharedResourceDirectoryName);
        }
    }
}
