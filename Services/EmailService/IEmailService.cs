using Microsoft.AspNetCore.Http;
using Models;

namespace Services.EmailService;

public interface IEmailService
{
    public Task SendMail(string to, string subject, string body, IFormFile? attachment = null);
    public Task SendTestReport(TestReport report, IFormFile zipFile);
}
