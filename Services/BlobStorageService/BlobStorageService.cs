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
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var blobClient = _blobContainerClient.GetBlobClient(fileName + ".zip");
        await blobClient.UploadAsync(stream, overwrite: true);
        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders {ContentType = file.ContentType});
        _logger.LogInformation("Uploaded file to blob storage");
        return blobClient.Uri.ToString();
    }
}
