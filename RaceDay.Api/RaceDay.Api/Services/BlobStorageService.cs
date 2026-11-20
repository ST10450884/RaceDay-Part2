
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            string containerName =
                configuration["AzureStorage:ContainerName"]
                ?? "event-images";

            _containerClient = new BlobContainerClient(
                connectionString,
                containerName);
        }

        public async Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType)
        {
            string extension = Path.GetExtension(fileName);

            string blobName = $"{Guid.NewGuid():N}{extension}";

            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(
                fileStream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                });

            return blobName;
        }

        public async Task<Stream?> DownloadAsync(string blobName)
        {
            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            try
            {
                var response = await blobClient.DownloadStreamingAsync();

                return response.Value.Content;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task DeleteAsync(string blobName)
        {
            BlobClient blobClient =
                _containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}
