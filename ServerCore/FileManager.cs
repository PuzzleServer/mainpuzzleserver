using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ServerCore
{
    public class FileManager
    {
        /// <summary>
        /// Uploads a file to blob storage
        /// </summary>
        /// <param name="fileName">Name of the file to upload. The caller should ensure it is safe to use in a URL</param>
        /// <param name="eventId">Event the file will be used in</param>
        /// <param name="contents">Contents of the file</param>
        /// <returns>Url of the file in blob storage</returns>
        public static async Task<Uri> UploadBlobAsync(string fileName, int eventId, Stream contents)
        {
            CloudBlobContainer eventContainer = await GetOrCreateEventContainerAsync(eventId);

            // Obfuscate the file by putting it in a random directory
            byte[] manglingBytes = new byte[16];
            RandomNumberGenerator.Fill(manglingBytes);
            // Turn the random bytes into legal URL characters
            string mangledString = Convert.ToBase64String(manglingBytes).Replace('/', '_');
            CloudBlobDirectory puzzleDirectory = eventContainer.GetDirectoryReference(mangledString);

            CloudBlockBlob blob = puzzleDirectory.GetBlockBlobReference(fileName);
            await blob.UploadFromStreamAsync(contents);
            return blob.Uri;
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
                // todo: read the account info from configuration
                return CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            }
        }
    }
}
