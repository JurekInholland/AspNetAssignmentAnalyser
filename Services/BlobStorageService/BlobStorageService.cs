using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Services.BlobStorageService;

public class BlobStorageService : IBlobStorageService
{
    private readonly ILogger<BlobStorageService> _logger;
    private readonly BlobContainerClient _blobContainerClient;

    public BlobStorageService(ILogger<BlobStorageService> logger, BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobContainerClient = blobServiceClient.GetBlobContainerClient("uploads");
        _blobContainerClient.CreateIfNotExists();
    }

    /// <summary>
    /// Upload a given file to blob storage with the given filename
    /// </summary>
    /// <returns>Uri to blob</returns>
    public async Task<string> UploadFile(MemoryStream stream, string fileName, string contentType)
    {
        stream.Position = 0;
        var blobClient = _blobContainerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(stream, overwrite: true);
        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders {ContentType = contentType});
        _logger.LogInformation("Uploaded file to blob storage");
        return blobClient.Uri.ToString();
    }
}
