using Microsoft.AspNetCore.SignalR;
using Models;

namespace Services;

public class SubmissionHub : Hub
{
    public async Task SendStatus(string user, string status, bool success = true)
    {
        StatusMessage message = new(status, success);
        await SendObjectToClient(user, "StatusMessage", message);
    }

    public async Task SendTestResult(string user, TestResult result)
    {
        StatusMessage message = new(result);
        await SendObjectToClient(user, "StatusMessage", message);
    }

    public async Task SendError(string user, Error error)
    {
        await SendObjectToClient(user, "ErrorMessage", error);
    }

    private async Task SendObjectToClient(string user, string callbackName, object obj)
    {
        await Clients.Client(user).SendAsync(callbackName, obj);
    }
}
