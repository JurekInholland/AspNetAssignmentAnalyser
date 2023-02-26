using Ionic.Zip;
using Microsoft.AspNetCore.Http;
using OpenQA.Selenium;
using Services.EmailService;
using Services.SnakeTestService;

namespace Services.FileUploadService;

public class FileUploadService : IFileUploadService
{
    private readonly SubmissionHub _hub;
    private readonly ISnakeTestService _snakeTestService;
    private readonly IEmailService _emailService;

    private const string CustomCode =
        "Window.Game.getFps = () => { return framesPerSecond; }; Window.Game.eatApple = eatApple; Window.Game.getPause = () => { return pauze; }; Window.Game.getScore = () => { return score; }})(Window.Game);";

    public FileUploadService(SubmissionHub hub, ISnakeTestService snakeTestService, IEmailService emailService)
    {
        _hub = hub;
        _snakeTestService = snakeTestService;
        _emailService = emailService;
    }

    public async Task SubmitFile(IFormCollection collection, int userId)
    {
        try
        {
            await HandleSubmission(collection);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            collection.TryGetValue("connectionId", out var connectionId);
            await _hub.SendStatus(connectionId!, $"Error: {e.Message}", false);
        }
        finally
        {
            Console.WriteLine("finally");
        }
    }

    private async Task HandleSubmission(IFormCollection collection)
    {
        collection.TryGetValue("connectionId", out var connectionId);
        if (string.IsNullOrEmpty(connectionId)) throw new InvalidDataException("ConnectionId is null or empty");

        var files = collection.Files.ToList();
        CheckSubmission(files);
        var file = files.First();

        await _hub.SendStatus(connectionId!, $"{files.Count} valid files received");

        await ProcessUpload(connectionId!, file);
    }

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
    private string ExtractZipFile(IFormFile file, string targetDirectory)
    {
        string path = Path.Combine("upload", targetDirectory);
        Directory.CreateDirectory(path);
        using var zip = ZipFile.Read(file.OpenReadStream());
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

    private void InjectJavascriptCode(string path)
    {
        var gameCode = File.ReadAllLines(path);
        gameCode[^1] = CustomCode;
        File.WriteAllLines(path, gameCode);
    }


    private async Task ProcessUpload(string connectionId, IFormFile file)
    {
        Guid id = Guid.NewGuid();
        var path = ExtractZipFile(file, id.ToString());
        await _hub.SendStatus(connectionId, "extraction complete");

        File.Copy("SnakeFiles/index.html", Path.Combine(path, "index.html"), overwrite: true);

        try
        {
            var report = await _snakeTestService.RunTests(path, connectionId);
            await _hub.SendStatus(connectionId, "Tests completed");
            await _emailService.SendTestReport(report, file);
        }
        catch (Exception e)
        {
            string msg = e is JavaScriptException ? "Error during test execution." : "Something went wrong";
            await _hub.SendStatus(connectionId, msg + e.Message, false);
        }
        finally
        {
            CleanUp(path);
        }

        Console.WriteLine("send mail");
    }

    private void CleanUp(string path)
    {
        Directory.Delete(path, true);
    }

    private bool IsFileTypeValid(IFormFile file)
    {
        return file.ContentType is "application/zip" or "application/x-zip-compressed";
    }

    private static bool IsFileSizeValid(IFormFile file)
    {
        return file.Length is > 50000 and <= 250000; // 50kb-250kb required file size?
    }
}
