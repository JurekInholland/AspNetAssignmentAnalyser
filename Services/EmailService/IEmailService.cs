using Microsoft.AspNetCore.Http;
using Models;

namespace Services.EmailService;

public interface IEmailService
{
    public Task SendMail(string to, string subject, string body);
    public Task SendTestReport(TestReport report, string fileUri);
}
