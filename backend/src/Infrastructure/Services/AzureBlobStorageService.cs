using Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobStorageService> _logger;

        public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            _logger = logger;

            var connectionString = configuration.GetConnectionString("AzureBlob");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:AzureBlob no esta configurado.");
            }

            var containerName = configuration["AzureBlobStorage:ContainerName"] ?? "reclamos-documentos";
            _containerClient = new BlobContainerClient(connectionString, containerName, new BlobClientOptions
            {
                Retry =
                {
                    Mode = Azure.Core.RetryMode.Exponential,
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(8),
                    MaxRetries = 5,
                    NetworkTimeout = TimeSpan.FromSeconds(100)
                }
            });
        }

        public async Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken cancellationToken = default)
        {
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var blobClient = _containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
                }
            }, cancellationToken);

            _logger.LogInformation("Documento subido a Blob Storage: {BlobPath}", blobPath);
            return blobPath;
        }

        public Task<string> GetUrlAsync(string blobPath, CancellationToken cancellationToken = default)
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);
            return Task.FromResult(blobClient.Uri.ToString());
        }

        public async Task DeleteAsync(string blobPath, CancellationToken cancellationToken = default)
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);

            _logger.LogInformation("Documento eliminado de Blob Storage: {BlobPath}", blobPath);
        }
    }
}
