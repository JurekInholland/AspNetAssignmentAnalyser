using HandlebarsDotNet;
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
    <h3>{{student}} has submitted the snake assignment</h3>
    <p>Submitted on {{submissionTime}}</p>
    {{#each items}}
        <p> <span style='font-weight: bold;'>{{this.number}}</span> - {{this.name}}: <span style='font-weight:bold;'>{{this.passed}}</span></p>
    {{/each}}
    <h3>Score: {{score}}% </h3>
    <a href=""{{fileUri}}"">Download submission</a>
    <p>Job id: {{id}}</p>
    <br />
    <p>Best regards,</p>
    <p>🐍 Snake assignment analyser</p>
</body>
</html>";


    public EmailService(ILogger<EmailService> logger, IOptions<AppConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    public async Task SendTestReport(TestReport report, string fileUri)
    {
        var html = CreateEmail(report, fileUri);
        await SendMail(_config.SendGridToEmail, report.Name, html);
    }

    private string CreateEmail(TestReport report, string fileUri)
    {
        var compiled = Handlebars.Compile(Template);

        var html = compiled(new
        {
            id = report.Id,
            title = "Test Report",
            heading = report.Name,
            items = report.Results,
            student = report.StudentEmail,
            score = report.Score,
            submissionTime = report.SubmissionTime.ToString("dd.MM.yy HH:mm:ss"),
            fileUri
        });

        if (html is null) throw new EmailException("Error during email creation");
        return html;
        // var engine = new RazorLightEngineBuilder()
        //     .UseEmbeddedResourcesProject(System.Reflection.Assembly.GetEntryAssembly())
        //     .UseMemoryCachingProvider()
        //     .Build();
        // // await engine.CompileRenderAsync("Headline", "Headline");
        // return engine.CompileRenderAsync("Body", report).Result;
    }

    // private static async Task<string> CreateAttachment(IFormFile file)
    // {
    //     byte[] fileBytes;
    //     using (var memoryStream = new MemoryStream())
    //     {
    //         await file.CopyToAsync(memoryStream);
    //         fileBytes = memoryStream.ToArray();
    //     }
    //
    //     return Convert.ToBase64String(fileBytes);
    // }

    public async Task SendMail(string to, string subject, string body)
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

        var response = await _client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
        {
            throw new EmailException(response.StatusCode.ToString());
        }

        _logger.LogInformation("Email sent");
    }
}

public class EmailException : Exception
{
    public EmailException(string msg) : base(msg)
    {
    }
}
