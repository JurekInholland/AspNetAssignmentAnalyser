using Microsoft.AspNetCore.Http;

namespace Services.Extensions;

public static class FormFileExtensions
{
    public static async Task<MemoryStream> GetStream(this IFormFile formFile)
    {
        var memoryStream = new MemoryStream();
        await formFile.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}
