using HandlebarsDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Services.EmailService;

public class EmailService : IEmailService
{
    private SendGridClient _client = null!;
    private readonly ILogger<EmailService> _logger;
    private readonly AppConfig _config;

    private const string Template = @"
<html>
<head>
    <title>{{title}}</title>
</head>
<body>
    <h1>{{heading}}</h1>
    {{#each items}}
        <p>{{this.number}} - {{this.name}} {{this.passed}}</p>
    {{/each}}
</body>
</html>";


    public EmailService(ILogger<EmailService> logger, IOptions<AppConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    public async Task SendTestReport(TestReport report, IFormFile zipFile)
    {
        var html = CreateEmail(report);
        await SendMail(_config.SendGridToEmail, report.Name, html, zipFile);
    }

    private string CreateEmail(TestReport report)
    {
        var compiled = Handlebars.Compile(Template);

        var html = compiled(new
        {
            title = "Test Report",
            heading = report.Name,
            items = report.Results
        });

        if (html is null) throw new Exception("Error during email creation");
        return html;
        // var engine = new RazorLightEngineBuilder()
        //     .UseEmbeddedResourcesProject(System.Reflection.Assembly.GetEntryAssembly())
        //     .UseMemoryCachingProvider()
        //     .Build();
        // // await engine.CompileRenderAsync("Headline", "Headline");
        // return engine.CompileRenderAsync("Body", report).Result;
    }

    private static async Task<string> CreateAttachment(IFormFile file)
    {
        byte[] fileBytes;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();
        }

        return Convert.ToBase64String(fileBytes);
    }

    public async Task SendMail(string to, string subject, string body, IFormFile? attachment = null)
    {
        _client = new SendGridClient(_config.SendGridApiKey);

        Console.WriteLine("Sending email");
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_config.SendGridFromEmail, "Assignment Analyser"),
            Subject = subject,
            HtmlContent = body,
        };
        msg.AddTo(new EmailAddress(to));

        // if (attachment is not null)
        // {
        //     msg.AddAttachment("attach", await CreateAttachment(attachment), type: "application/pdf" );
        // }

        var response = await _client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode) throw new Exception("Email was not sent");
        _logger.LogInformation("Email sent");
    }
}
