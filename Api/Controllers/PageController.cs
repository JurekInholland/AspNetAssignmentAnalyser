using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services;

namespace Api.Controllers;

[Route("")]
public class PageController : BaseController
{
    private static readonly string IndexHtml = System.IO.File.ReadAllText("wwwroot/index.html");
    private readonly IHubContext<SubmissionHub> _hubContext;

    public PageController(IHubContext<SubmissionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpGet(Name = nameof(Index))]
    public async Task<IActionResult> Index()
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "testmessage");
        return new ContentResult
        {
            Content = IndexHtml,
            ContentType = "text/html",
            StatusCode = 200
        };
    }
}
