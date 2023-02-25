using Ionic.Zip;
using Microsoft.AspNetCore.Http;
using Services.SnakeTestService;

namespace Services.FileUploadService;

public class FileUploadService : IFileUploadService
{
    private readonly SubmissionHub _hub;
    private readonly ISnakeTestService _snakeTestService;

    public FileUploadService(SubmissionHub hub, ISnakeTestService snakeTestService)
    {
        _hub = hub;
        _snakeTestService = snakeTestService;
    }

    public async Task SubmitFile(IFormCollection collection)
        // public async Task SubmitFile(List<IFormFile> files, string connectionId)
    {
        collection.TryGetValue("connectionId", out var connectionId);

        if (string.IsNullOrEmpty(connectionId)) throw new InvalidDataException("ConnectionId is null or empty");

        var files = collection.Files.ToList();

        await _hub.SendMessageToClient(connectionId!, $"{files.Count} files received");

        if (files.Count != 1) throw new InvalidDataException("Only one file is allowed");
        if (!IsFileTypeValid(files[0])) throw new InvalidDataException("Invalid file type");
        if (!IsFileSizeValid(files[0])) throw new InvalidDataException("Invalid file size");
        await _hub.SendMessageToClient(connectionId!, $"{files[0].Length}: files size is valid");

        var id = ExtractZipFile(files[0]);
        await _hub.SendMessageToClient(connectionId!, $"extraction complete");

        _ = Task.Run(() => ProcessFiles(id, connectionId!));
        await _hub.SendMessageToClient(connectionId!, $"running tests");

    }

    private Guid ExtractZipFile(IFormFile file)
    {
        Guid id = Guid.NewGuid();
        string path = Path.Combine("upload", id.ToString());
        Directory.CreateDirectory(path);
        using var zip = ZipFile.Read(file.OpenReadStream());
        zip.FlattenFoldersOnExtract = true;

        foreach (var entry in zip)
        {
            entry.Extract(path,
                ExtractExistingFileAction.OverwriteSilently);
        }
        return id;
    }

    private void ProcessFiles(Guid id, string connectionId)
    {
        string path = Path.Combine("upload", id.ToString());
        File.Copy("SnakeFiles/index.html", Path.Combine(path, "index.html"), overwrite: true);

        _snakeTestService.RunTests(path, connectionId);
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
