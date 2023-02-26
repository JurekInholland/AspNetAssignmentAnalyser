using Microsoft.AspNetCore.Mvc;
using Services.EmailService;
using Services.FileUploadService;

namespace Api.Controllers;

public class UploadController : BaseController
{
    private readonly IFileUploadService _fileUploadService;
    private readonly IEmailService _emailService;

    public UploadController(IFileUploadService fileUploadService, IEmailService emailService)
    {
        _fileUploadService = fileUploadService;
        _emailService = emailService;
    }

    [HttpPost("", Name = nameof(UploadZip))]
    public async Task<IActionResult> UploadZip(IFormCollection collection)
    {
        int userId = ParseUserId();

        await Task.Run(() => { _fileUploadService.SubmitFile(collection, userId); });
        return new OkObjectResult("Test started");
    }

    private int ParseUserId()
    {
        if (Request.Headers.TryGetValue("x-userid", out var userId))
        {
            var domainName = userId.ToString().Split("@").First();
            if (int.TryParse(domainName, out var id))
                return id;
        }

        return 12345;
    }

    [HttpGet("test", Name = nameof(TestSndMail))]
    public async Task<IActionResult> TestSndMail()
    {
        await _emailService.SendMail("jurekstuff@gmail.com", "hello", "lello schmello");

        return new OkObjectResult("Email sent");
    }
}
