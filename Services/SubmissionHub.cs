using Microsoft.AspNetCore.SignalR;

namespace Services;

public class SubmissionHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        Console.WriteLine("SubmissionHub SendMessage" + user + message);
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task SendMessageToClient(string user, string message)
    {
        Console.WriteLine("SubmissionHub SendMessageToClient" + user + message);
        await Clients.Client(user).SendAsync("ReceiveMessage", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("SubmissionHub OnConnectedAsync");
        await Clients.All.SendAsync("ReceiveMessage", "New connection");
    }
}
