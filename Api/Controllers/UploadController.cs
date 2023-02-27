using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services;
using Services.EmailService;
using Services.FileUploadService;

namespace Api.Controllers;

public class UploadController : BaseController
{
    private readonly ILogger<UploadController> _logger;
    private readonly IFileUploadService _fileUploadService;
    private readonly IEmailService _emailService;
    private readonly AppConfig _config;

    public UploadController(ILogger<UploadController> logger, IFileUploadService fileUploadService, IEmailService emailService,
        IOptions<AppConfig> config)
    {
        _logger = logger;
        _fileUploadService = fileUploadService;
        _emailService = emailService;
        _config = config.Value;
    }

    [HttpPost("", Name = nameof(UploadZip))]
    public async Task<IActionResult> UploadZip(IFormCollection collection)
    {
        var userEmail = ParseRequestHeader(_config.UserHeaderKey) ?? "Unkown user";
        await _fileUploadService.SubmitFile(collection, userEmail);
        return new OkObjectResult("Test started");
    }

    private string? ParseRequestHeader(string headerKey)
    {
        // DebugLogReqHeaders();
        return Request.Headers.TryGetValue(headerKey, out var headerValue) ? headerValue.ToString() : null;
    }

    private void DebugLogReqHeaders()
    {
        _logger.LogInformation("Parsing request header");
        foreach (var header in Request.Headers)
        {
            _logger.LogInformation("{HeaderKey}: {HeaderValue}", header.Key, header.Value);
        }
    }

    [HttpGet("test", Name = nameof(TestSndMail))]
    public async Task<IActionResult> TestSndMail()
    {
        await _emailService.SendMail("jurekstuff@gmail.com", "hello", "lello schmello");

        return new OkObjectResult("Email sent");
    }
}
