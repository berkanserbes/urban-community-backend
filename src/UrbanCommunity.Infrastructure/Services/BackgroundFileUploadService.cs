using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Services;

public sealed class BackgroundFileUploadService : IBackgroundFileUploadService
{
    private static readonly Dictionary<string, long> AllowedExtensions = new()
    {
        // Resim
        { ".jpg",  10  * 1024 * 1024 },
        { ".jpeg", 10  * 1024 * 1024 },
        { ".png",  10  * 1024 * 1024 },
        { ".webp", 10  * 1024 * 1024 },
        { ".gif",  10  * 1024 * 1024 },

        // Video
        { ".mp4",  500 * 1024 * 1024 },
        { ".mov",  500 * 1024 * 1024 },
        { ".avi",  500 * 1024 * 1024 },
        { ".webm", 500 * 1024 * 1024 },

        // Belge
        { ".pdf",  50  * 1024 * 1024 },
    };

    private readonly IPublishEndpoint _publishEndpoint;
    private readonly string _tempRoot;

    public BackgroundFileUploadService(
        IPublishEndpoint publishEndpoint,
        IHostEnvironment env)
    {
        _publishEndpoint = publishEndpoint;
        _tempRoot = Path.Combine(env.ContentRootPath, "temp", "uploads");
    }

    public async Task<Guid> EnqueueAsync(
        IFormFile file,
        string folderName,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        var correlationId = Guid.CreateVersion7();

        string? tempPath = null;
        try
        {
            tempPath = await SaveToTempAsync(file, correlationId, cancellationToken);

            await _publishEndpoint.Publish(new FileUploadRequestedEvent
            {
                CorrelationId = correlationId,
                UserId = userId,
                TempFilePath = tempPath,
                OriginalFileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                FolderName = folderName
            }, cancellationToken);
        }
        catch
        {
            if (tempPath is not null && File.Exists(tempPath))
                File.Delete(tempPath);

            throw;
        }

        return correlationId;
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
            throw new InvalidInputException("Dosya boş olamaz.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.TryGetValue(ext, out var maxSize))
            throw new InvalidFileTypeException(
                attemptedContentType: ext,
                allowedTypes: [.. AllowedExtensions.Keys]);

        if (file.Length > maxSize)
            throw new FileTooLargeException(file.Length, maxSize);
    }

    private async Task<string> SaveToTempAsync(
        IFormFile file,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_tempRoot);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var tempPath = Path.Combine(_tempRoot, $"{correlationId}{ext}");

        await using var stream = new FileStream(
            tempPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await file.CopyToAsync(stream, cancellationToken);

        return tempPath;
    }
}