using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
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
    public async Task<string> UploadFile(IFormFile file, string fileName)
    {
        _logger.LogInformation("Uploading file to blob storage");
        var blobClient = _blobContainerClient.GetBlobClient(fileName + ".zip");
        await blobClient.UploadAsync(file.OpenReadStream());
        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders {ContentType = file.ContentType});
        return blobClient.Uri.ToString();
    }
}
