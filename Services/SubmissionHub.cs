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

    private async Task SendObjectToClient(string user, string callbackName, object obj)
    {
        Console.WriteLine("SubmissionHub SendObjectToClient" + user + obj);
        await Clients.Client(user).SendAsync(callbackName, obj);
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("SubmissionHub OnConnectedAsync");
        await Clients.All.SendAsync("ReceiveMessage", "New connection");
    }
}
