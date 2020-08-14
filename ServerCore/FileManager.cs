using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
        /// <returns>Url of the file in blob storage</returns>
        public static async Task<Uri> UploadBlobAsync(string fileName, int eventId, Stream contents)
        {
            CloudBlockBlob blob = await CreateNewBlob(fileName, eventId);

            await blob.UploadFromStreamAsync(contents);
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
            CloudBlockBlob blobSource = new CloudBlockBlob(sourceUri, StorageAccount.Credentials);
            
            CloudBlockBlob blob = await CreateNewBlob(fileName, eventId);
            await blob.StartCopyAsync(blobSource);

            return blob.Uri;
        }

        private static async Task<CloudBlockBlob> CreateNewBlob(string fileName, int eventId)
        {
            CloudBlobDirectory puzzleDirectory = await GetRandomDirectoryAsync(eventId);

            CloudBlockBlob blob = puzzleDirectory.GetBlockBlobReference(fileName);
            if (fileExtensionProvider.TryGetContentType(fileName, out string contentType))
            {
                blob.Properties.ContentType = contentType;
            }

            return blob;
        }

        /// <summary>
        /// Uploads a set of blobs to the same directory
        /// </summary>
        /// <param name="files">A dictionary of files. Keys are file names. The caller should ensure they are safe to use in a URL.
        /// The values are streams with the contents of the files</param>
        /// <param name="eventId">Event the files will be used in</param>
        /// <returns>A dictionary with key of the file names and value of the urls of the files in blob storage</returns>
        public static async Task<Dictionary<string, Uri>> UploadBlobsAsync(Dictionary<string, Stream> files, int eventId)
        {
            CloudBlobDirectory puzzleDirectory = await GetRandomDirectoryAsync(eventId);
            Dictionary<string, Uri> fileUrls = new Dictionary<string, Uri>(files.Count);

            foreach (KeyValuePair<string, Stream> file in files)
            {
                CloudBlockBlob blob = puzzleDirectory.GetBlockBlobReference(file.Key);
                if (fileExtensionProvider.TryGetContentType(file.Key, out string contentType))
                {
                    blob.Properties.ContentType = contentType;
                }

                await blob.UploadFromStreamAsync(file.Value);

                fileUrls[file.Key] = blob.Uri;
            }

            return fileUrls;
        }

        /// <summary>
        /// Deletes a file from blob storage
        /// </summary>
        /// <param name="fileUri">Full URL of the file to delete</param>
        public static async Task DeleteBlobAsync(Uri fileUri)
        {
            CloudBlob blobToDelete = new CloudBlob(fileUri, StorageAccount.Credentials);
            await blobToDelete.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Gets a reference to a random directory
        /// </summary>
        /// <param name="eventId">Event the directory should be associated with</param>
        private static async Task<CloudBlobDirectory> GetRandomDirectoryAsync(int eventId)
        {
            CloudBlobContainer eventContainer = await GetOrCreateEventContainerAsync(eventId);

            // Obfuscate the file by putting it in a random directory
            byte[] manglingBytes = new byte[16];
            RandomNumberGenerator.Fill(manglingBytes);
            // Turn the random bytes into legal URL characters
            string mangledString = Convert.ToBase64String(manglingBytes).Replace('/', '_');
            CloudBlobDirectory puzzleDirectory = eventContainer.GetDirectoryReference(mangledString);
            return puzzleDirectory;
        }

        /// <summary>
        /// Ensure a container exists for the event
        /// </summary>
        private static async Task<CloudBlobContainer> GetOrCreateEventContainerAsync(int eventId)
        {
            CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();

            CloudBlobContainer eventContainer = blobClient.GetContainerReference($"evt{eventId}");
            await eventContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, null, null);
            return eventContainer;
        }

        private static CloudStorageAccount StorageAccount
        {
            get
            { 
                return CloudStorageAccount.Parse(ConnectionString);
            }
        }
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
