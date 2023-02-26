using Microsoft.AspNetCore.Http;

namespace Services.FileUploadService;

public interface IFileUploadService
{
    public Task SubmitFile(IFormCollection collection, string userEmail);
}
