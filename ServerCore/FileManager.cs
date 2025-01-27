using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;

namespace ServerCore
{
    public class FileManager
    {
        public static string ConnectionString { get; set; }

        static readonly FileExtensionContentTypeProvider fileExtensionProvider = new FileExtensionContentTypeProvider();

        /// <summary>
        /// Uploads a file to blob storage
        /// </summary>
        /// <param name="fileName">Name of the file to upload. The caller should ensure it is safe to use in a URL</param>
        /// <param name="eventId">Event the file will be used in</param>
        /// <param name="contents">Contents of the file</param>
        /// <param name="specificDirectory">A directory name to upload the file to. If null, a randomized directory will be used.</param>
        /// <returns>Url of the file in blob storage</returns>
        public static async Task<Uri> UploadBlobAsync(string fileName, int eventId, Stream contents, string specificDirectory = null)
        {
            BlobContainerClient containerClient = await GetOrCreateEventContainerAsync(eventId);
            string directory = specificDirectory ?? GetRandomDirectoryName();
            string blobPath = $"{directory}/{fileName}";

            return await UploadBlobToContainerAsync(fileName, contents, containerClient, blobPath);
        }

        private static async Task<Uri> UploadBlobToContainerAsync(string fileName, Stream contents, BlobContainerClient containerClient, string blobPath)
        {
            BlobClient blob = containerClient.GetBlobClient(blobPath);
            await blob.UploadAsync(contents);

            if (fileExtensionProvider.TryGetContentType(fileName, out string contentType))
            {
                await blob.SetHttpHeadersAsync(new BlobHttpHeaders() { ContentType = contentType });
            }

            return blob.Uri;
        }

        /// <summary>
        /// Copies a file in blob storage
        /// </summary>
        /// <param name="fileName">Name of the file to upload. The caller should ensure it is safe to use in a URL</param>
        /// <param name="eventId">Event the file will be used in</param>
        /// <param name="sourceUri">Uri of the blob to be copied</param>
        /// <returns>Url of the file in blob storage</returns>
        public static async Task<Uri> CloneBlobAsync(string fileName, int eventId, Uri sourceUri)
        {
            BlobUriBuilder uriBuilder = new BlobUriBuilder(sourceUri);
            BlobClient destinationBlob = new BlobClient(ConnectionString, $"evt{eventId}", uriBuilder.BlobName);
            await destinationBlob.StartCopyFromUriAsync(sourceUri);

            return destinationBlob.Uri;
        }

        /// <summary>
        /// Enumerates all the files in a directory subtree.
        /// </summary>
        /// <param name="eventId">Event to find the directory in</param>
        /// <param name="directoryName">The name of the directory</param>
        /// <returns>The names and URIs of all the files in the directory.</returns>
        public static async Task<List<DirectoryFileResult>> GetDirectoryContents(int eventId, string directoryName)
        {
            List<DirectoryFileResult> results = new List<DirectoryFileResult>();
            BlobContainerClient eventContainer = await GetOrCreateEventContainerAsync(eventId);
            var blobs = eventContainer.GetBlobsByHierarchyAsync(prefix: directoryName);
            await foreach(var blob in blobs)
            {
                BlobUriBuilder uriBuilder = new BlobUriBuilder(eventContainer.Uri);
                uriBuilder.BlobName = blob.Blob.Name;
                
                results.Add(new DirectoryFileResult() { Name = blob.Blob.Name, Uri = uriBuilder.ToUri() });
            }

            return results;
        }

        /// <summary>
        /// Uploads a set of blobs to the same directory
        /// </summary>
        /// <param name="files">A dictionary of files. Keys are file names. The caller should ensure they are safe to use in a URL.
        /// The values are streams with the contents of the files</param>
        /// <param name="eventId">Event the files will be used in</param>
        /// <param name="specificDirectory">A directory name to upload the files to. If null, a randomized directory will be used.</param>
        /// <returns>A dictionary with key of the file names and value of the urls of the files in blob storage</returns>
        public static async Task<Dictionary<string, Uri>> UploadBlobsAsync(Dictionary<string, Stream> files, int eventId, string specificDirectory = null)
        {
            BlobContainerClient containerClient = await GetOrCreateEventContainerAsync(eventId);
            string directory = specificDirectory ?? GetRandomDirectoryName();
            Dictionary<string, Uri> fileUrls = new Dictionary<string, Uri>(files.Count);

            foreach (KeyValuePair<string, Stream> file in files)
            {
                string fullPath = $"{directory}/{file.Key}";
                fileUrls[file.Key] = await UploadBlobToContainerAsync(file.Key, file.Value, containerClient, fullPath);
            }

            return fileUrls;
        }

        /// <summary>
        /// Deletes a file from blob storage
        /// </summary>
        /// <param name="fileUri">Full URL of the file to delete</param>
        public static async Task DeleteBlobAsync(Uri fileUri)
        {
            BlobUriBuilder uriBuilder = new BlobUriBuilder(fileUri);
            BlobClient blobToDelete = new BlobClient(ConnectionString, uriBuilder.BlobContainerName, uriBuilder.BlobName);
            await blobToDelete.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Gets the url that that all file storage for this event lives at
        /// </summary>
        /// <returns>The url as a string</returns>
        public static string GetFileStoragePrefix(int eventId, string puzzleDirectoryName) {
            BlobContainerClient eventContainer = new BlobContainerClient(ConnectionString, $"evt{eventId}");
            return eventContainer.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Cretes a random directory name that will work in a URL
        /// </summary>
        private static string GetRandomDirectoryName()
        {
            byte[] manglingBytes = new byte[16];
            RandomNumberGenerator.Fill(manglingBytes);
            // Turn the random bytes into legal URL characters
            string mangledString = Convert.ToBase64String(manglingBytes).Replace('/', '_');
            return mangledString;
        }

        /// <summary>
        /// Ensure a container exists for the event
        /// </summary>
        private static async Task<BlobContainerClient> GetOrCreateEventContainerAsync(int eventId)
        {
            BlobContainerClient eventContainer = new BlobContainerClient(ConnectionString, $"evt{eventId}");
            await eventContainer.CreateIfNotExistsAsync(PublicAccessType.Blob);
            return eventContainer;
        }
    }

    /// <summary>
    /// One file in the result of a call to GetDirectoryContents.
    /// </summary>
    public class DirectoryFileResult
    {
        public string Name { get; set; }
        public Uri Uri { get; set; }
    }

    /// <summary>
    /// Uploads files in the background and Saves them to the database when the upload is complete
    /// </summary>
    public class BackgroundFileUploader
    {
        PuzzleServerContext _context;

        public BackgroundFileUploader(IServiceProvider serviceProvider)
        {
            IServiceScope newScope = serviceProvider.CreateScope();
            _context = newScope.ServiceProvider.GetService<PuzzleServerContext>();
        }

        public void CloneInBackground(ContentFile newFile, string fileName, int eventId, Uri sourceUri)
        {
            Task<Uri> t = FileManager.CloneBlobAsync(fileName, eventId, sourceUri);
            t.ContinueWith((finishedTask) =>
            {
                newFile.Url = finishedTask.Result;
                lock (_context)
                {
                    _context.ContentFiles.Add(newFile);
                    _context.SaveChanges();
                }
            });
        }
    }
}
