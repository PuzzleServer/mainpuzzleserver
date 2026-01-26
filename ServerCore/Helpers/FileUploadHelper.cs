using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using ServerCore.DataModel;

namespace ServerCore.Helpers
{
    public static class FileUploadHelper
    {
        /// <summary>
        /// Uploads files from the NewEventTemplate folder into a new event - resources plus an example puzzle
        /// </summary>
        public static async Task UploadNewEventFilesAsync(HttpContext httpContext, PuzzleServerContext context, int eventId, IWebHostEnvironment webHostEnvironment)
        {
            try
            {
                string templateRoot = Path.Combine(webHostEnvironment.WebRootPath, "NewEventTemplate");
                PhysicalFileProvider physicalFileProvider = new PhysicalFileProvider(templateRoot);

                // copy resources verbatim
                foreach (var file in EnumerateFilesRecursive(physicalFileProvider, "resources"))
                {
                    using (Stream s = file.CreateReadStream())
                    {
                        await FileManager.UploadBlobAsync(file.Name, eventId, s, Path.GetRelativePath(templateRoot, Path.GetDirectoryName(file.PhysicalPath)));
                    }
                }

                // Create and upload an example puzzle
                Puzzle examplePuzzle = new Puzzle() { EventID = eventId, Name = "Example Puzzle", IsPuzzle = true };
                await UploadPuzzleTemplateAsync(httpContext, context, examplePuzzle, physicalFileProvider, "example-puzzle", ContentFileType.PuzzleMaterial);
                await UploadPuzzleTemplateAsync(httpContext, context, examplePuzzle, physicalFileProvider, "example-puzzle-solution", ContentFileType.SolveToken);
            }
            catch (System.Exception)
            {
                // If this fails to create the files, we can ignore it and continue.
                // This can happen if the Azure storage simulator is not running on a local deployment.
            }
        }

        /// <summary>
        /// Helper for taking an uploaded form file, uploading it, and tracking it in the database
        /// </summary>
        public static async Task UploadFileAsync(HttpContext httpContext, PuzzleServerContext context, Puzzle puzzle, IFormFile uploadedFile, ContentFileType fileType)
        {
            Dictionary<string, Stream> contents = new Dictionary<string, Stream>();

            contents[uploadedFile.FileName] = uploadedFile.OpenReadStream();
            await UploadPuzzleStreamsAsync(httpContext, context, puzzle, contents, fileType);
        }

        /// <summary>
        /// Extracts the contents of a zip file and uploads the contents into a single directory
        /// </summary>
        public static async Task UploadZipAsync(HttpContext httpContext, PuzzleServerContext context, Puzzle puzzle, IFormFile uploadedFile, ContentFileType fileType)
        {
            ZipArchive archive = new ZipArchive(uploadedFile.OpenReadStream(), ZipArchiveMode.Read);
            Dictionary<string, Stream> contents = new Dictionary<string, Stream>();
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
                contents[fileName] = entry.Open();
            }

            await UploadPuzzleStreamsAsync(httpContext, context, puzzle, contents, fileType);
        }

        private static async Task UploadPuzzleTemplateAsync(HttpContext httpContext, PuzzleServerContext context, Puzzle puzzle, PhysicalFileProvider physicalFileProvider, string directory, ContentFileType fileType)
        {
            Dictionary<string, Stream> contents = new Dictionary<string, Stream>();

            foreach (var file in EnumerateFilesRecursive(physicalFileProvider, directory))
            {
                contents[Path.GetRelativePath(physicalFileProvider.Root, file.PhysicalPath).Replace('\\', '/')] = file.CreateReadStream();
            }

            await UploadPuzzleStreamsAsync(httpContext, context, puzzle, contents, fileType);
        }

        private static async Task UploadPuzzleStreamsAsync(HttpContext httpContext, PuzzleServerContext context, Puzzle puzzle, Dictionary<string, Stream> streams, ContentFileType fileType)
        {
            Dictionary<string, Uri> fileUrls = await FileManager.UploadBlobsAsync(streams, puzzle.EventID);
            foreach (KeyValuePair<string, Uri> fileUrl in fileUrls)
            {
                ContentFile file = new ContentFile()
                {
                    ShortName = fileUrl.Key,
                    Puzzle = puzzle,
                    EventID = puzzle.EventID,
                    FileType = fileType,
                    Url = fileUrl.Value,
                };
                UpdateCustomURLs(httpContext, puzzle, file);
                context.ContentFiles.Add(file);
            }
        }

        private static IEnumerable<IFileInfo> EnumerateFilesRecursive(IFileProvider provider, string subpath)
        {
            IDirectoryContents contents;
            try
            {
                contents = provider.GetDirectoryContents(subpath);
            }
            catch
            {
                yield break; // Invalid path or inaccessible
            }

            if (!contents.Exists)
                yield break;

            foreach (var item in contents)
            {
                if (item.IsDirectory)
                {
                    // Recursively enumerate subdirectories
                    foreach (var subItem in EnumerateFilesRecursive(provider, Path.Combine(subpath, item.Name)))
                    {
                        yield return subItem;
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Makes a clean link to the ContentFile based on ShortName, that is not specific to this event.
        /// </summary>
        /// <param name="file">The ContentFile</param>
        /// <returns>a link that replaces the explicit eventId with {eventId}, best used for Puzzle.CustomURL and Puzzle.CustomSolutionURL.</returns>
        private static string EventAgnosticLinkFromShortName(HttpContext httpContext, ContentFile file)
        {
            // lack of translation of {eventId} is very intentional - this is translated at runtime and makes the field value portable
            return httpContext.Request.Scheme + "://" + httpContext.Request.Host + "/{eventId}/Files/" + file.ShortName;
        }

        /// <summary>
        /// Promotes files named "index.html" to the CustomURL or CustomSolutionURL properties of the puzzle.
        /// </summary>
        /// <param name="file">The file being uploaded</param>
        private static void UpdateCustomURLs(HttpContext httpContext, Puzzle puzzle, ContentFile file)
        {
            // Skip this if:
            // - the file is not a material or solution file
            // - the appropriate Custom[Solution]URL is non-blank
            if (!(file.FileType == ContentFileType.PuzzleMaterial || file.FileType == ContentFileType.SolveToken) ||
                (file.FileType == ContentFileType.PuzzleMaterial && !string.IsNullOrEmpty(puzzle.CustomURL)) ||
                (file.FileType == ContentFileType.SolveToken && !string.IsNullOrEmpty(puzzle.CustomSolutionURL)))
            {
                return;
            }

            string fileNameLower = Path.GetFileName(file.ShortName).ToLower();
            if (fileNameLower == "index.html")
            {
                string filePath = EventAgnosticLinkFromShortName(httpContext, file);
                if (file.FileType == ContentFileType.PuzzleMaterial)
                {
                    puzzle.CustomURL = filePath;
                }
                else
                {
                    puzzle.CustomSolutionURL = filePath;
                }
            }
            else if (fileNameLower.EndsWith("_client.html") && file.FileType == ContentFileType.PuzzleMaterial)
            {
                puzzle.CustomURL = httpContext.Request.Scheme + "://" + httpContext.Request.Host + "/{eventId}/{puzzleId}/api/Sync/client/?eventId={eventId}";
            }
        }
    }
}
