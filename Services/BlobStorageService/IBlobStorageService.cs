namespace Services.BlobStorageService;

public interface IBlobStorageService
{
    public Task<string> UploadFile(MemoryStream stream, string fileName, string contentType);
}
