using Microsoft.AspNetCore.Http;

namespace Services.BlobStorageService;

public interface IBlobStorageService
{
    public Task<string> UploadFile(IFormFile file, string fileName);
}
