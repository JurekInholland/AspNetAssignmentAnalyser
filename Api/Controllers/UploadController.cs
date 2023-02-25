using Microsoft.AspNetCore.Mvc;
using Services.FileUploadService;

namespace Api.Controllers;

public class UploadController : BaseController
{
    private readonly IFileUploadService _fileUploadService;

    public UploadController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpPost("", Name = nameof(UploadZip))]
    public async Task<IActionResult> UploadZip(IFormCollection collection)
    {
        await Task.Run(() => { _fileUploadService.SubmitFile(collection); });
        return new OkObjectResult("Test started");
    }
}
