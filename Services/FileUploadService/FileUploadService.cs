using Azure;
using Ionic.Zip;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.BlobStorageService;
using Services.EmailService;
using Services.Extensions;
using Services.SnakeTestService;

namespace Services.FileUploadService;

public class FileUploadService : IFileUploadService
{
    private readonly ILogger<FileUploadService> _logger;
    private readonly SubmissionHub _hub;
    private readonly ISnakeTestService _snakeTestService;
    private readonly IEmailService _emailService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IApplicationLifetime _applicationLifetime;

    private const string CustomCode =
        "Window.Game.getFps = () => { return frameCounterLimit; }; Window.Game.eatApple = eatApple; Window.Game.getPause = () => { return pauze; }; Window.Game.getScore = () => { return score; }})(Window.Game);";

    private string _connectionId = "";

    public FileUploadService(ILogger<FileUploadService> logger, SubmissionHub hub, ISnakeTestService snakeTestService,
        IEmailService emailService,
        IBlobStorageService blobStorageService, IApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _hub = hub;
        _snakeTestService = snakeTestService;
        _emailService = emailService;
        _blobStorageService = blobStorageService;
        _applicationLifetime = applicationLifetime;
    }

    /// <summary>
    ///    Submit a file to the server; This method is run in a background thread
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="userEmail"></param>
    public async Task SubmitFile(IFormCollection collection, string userEmail)
    {
        CheckSubmission(collection.Files.ToList());
        if (!collection.TryGetValue("connectionId", out var connectionId)) throw new InvalidDataException("ConnectionId is null or empty");
        _connectionId = connectionId.ToString();

        var stream = await collection.Files[0].GetStream();
        ExecuteInBackground(stream, userEmail);
    }


    /// <summary>
    /// Safely execute the file upload/processing in a background thread
    /// </summary>
    private void ExecuteInBackground(MemoryStream stream, string userEmail)
    {
        _applicationLifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () => await ProcessUpload(_connectionId, stream, userEmail)).ContinueWith(async task =>
            {
                if (task.IsFaulted)
                {
                    _logger.LogError("BACKGROUND TASK FAILED: {TaskException}", task.Exception);

                    // collection.TryGetValue("connectionId", out var connectionId);
                    await _hub.SendStatus(_connectionId, $"Error: {task.Exception?.Message}", false);
                    throw new Exception("BACKGROUND TASK FAILED", task.Exception);
                }
            });
        });
    }

    // private async Task HandleSubmission(MemoryStream stream, string userEmail)
    // {
    //     if (!collection.TryGetValue("connectionId", out var connectionId)) throw new InvalidDataException("ConnectionId is null or empty");
    //
    //     var files = collection.Files.ToList();
    //     CheckSubmission(files);
    //     var file = files.First();
    //
    //     await _hub.SendStatus(connectionId!, $"{files.Count} valid files received");
    //
    //     await ProcessUpload(connectionId!, file, userEmail);
    // }

    private void CheckSubmission(IReadOnlyCollection<IFormFile> files)
    {
        if (files.Count != 1) throw new InvalidDataException("Only one file is allowed");
        if (!IsFileTypeValid(files.First())) throw new InvalidDataException("Invalid files.First() type");
        if (!IsFileSizeValid(files.First())) throw new InvalidDataException("Invalid files.First() size");
    }

    /// <summary>
    /// Extract a given zip file into a given directory
    /// </summary>
    /// <returns>Absolute path to the extracted directory</returns>
    private string ExtractZipFile(MemoryStream stream, string targetDirectory)
    {
        string path = Path.Combine("upload", targetDirectory);
        Directory.CreateDirectory(path);
        using var zip = ZipFile.Read(stream);
        zip.FlattenFoldersOnExtract = true;

        foreach (var entry in zip)
        {
            entry.Extract(path,
                ExtractExistingFileAction.OverwriteSilently);
            if (Path.GetFileName(entry.FileName) is "game.js")
                InjectJavascriptCode(Path.Combine(path, "game.js"));
        }

        return Path.GetFullPath(path);
    }

    private static void InjectJavascriptCode(string path)
    {
        var gameCode = File.ReadAllLines(path);
        gameCode[^1] = CustomCode;
        File.WriteAllLines(path, gameCode);
    }


    private async Task ProcessUpload(string connectionId, MemoryStream stream, string userEmail)
    {
        Guid id = Guid.NewGuid();
        await _hub.SendStatus(connectionId, "extracting files");
        var path = ExtractZipFile(stream, id.ToString());

        File.Copy("SnakeFiles/index.html", Path.Combine(path, "index.html"), overwrite: true);

        try
        {
            var report = await _snakeTestService.RunTests(path, connectionId);
            report.StudentEmail = userEmail;
            await _hub.SendStatus(connectionId, "done");


            var fileName = id + ".zip";
            var uri = await _blobStorageService.UploadFile(stream, fileName, "application/x-zip-compressed");

            await _emailService.SendTestReport(report, uri);
        }
        catch (Exception e)
        {
            string msg = e switch
            {
                JavaScriptException => "Error during test execution: ",
                InvalidCastException => "Unable to run tests: ",
                EmailException => "Unable to send email: ",
                RequestFailedException => "Unable to upload file: ",
                _ => ""
            };
            if (msg == "")
            {
                Console.WriteLine("unknown error:" + e);
            }

            await _hub.SendStatus(connectionId, msg + e.Message, false);
        }
        finally
        {
            CleanUp(path);
        }
    }

    private static void CleanUp(string path)
    {
        Directory.Delete(path, true);
    }

    private bool IsFileTypeValid(IFormFile file)
    {
        return file.ContentType is "application/zip" or "application/x-zip-compressed";
    }

    private static bool IsFileSizeValid(IFormFile file)
    {
        return file.Length is > 50000 and <= 200000; // 50kb-200kb required file size?
    }
}
