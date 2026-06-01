using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Consumers;

/// <summary>
/// Processes large file upload requests in the background.
/// Reads the file from temporary storage, uploads it to permanent storage,
/// and notifies the user via SignalR upon completion or failure.
/// </summary>
public sealed class FileUploadConsumer(
    IFileService fileService,
    IRealtimeNotificationService notificationService,
    ILogger<FileUploadConsumer> logger) : IConsumer<FileUploadRequestedEvent>
{
    public async Task Consume(ConsumeContext<FileUploadRequestedEvent> consumeContext)
    {
        var evt = consumeContext.Message;

        try
        {
            var url = await UploadFromTempAsync(evt, consumeContext.CancellationToken);

            await notificationService.SendToUserAsync(
                evt.UserId.ToString(),
                "FileUploadCompleted",
                new { correlationId = evt.CorrelationId, url, success = true },
                consumeContext.CancellationToken);

            logger.LogInformation(
                "Dosya arka planda yüklendi. CorrelationId: {CorrelationId}, Url: {Url}",
                evt.CorrelationId, url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Arka plan dosya yükleme başarısız. CorrelationId: {CorrelationId}",
                evt.CorrelationId);

            await notificationService.SendToUserAsync(
                evt.UserId.ToString(),
                "FileUploadFailed",
                new { correlationId = evt.CorrelationId, error = "Dosya yüklenirken bir hata oluştu." },
                consumeContext.CancellationToken);
        }
        finally
        {
            DeleteTempFile(evt.TempFilePath);
        }
    }

    private async Task<string> UploadFromTempAsync(FileUploadRequestedEvent evt, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(evt.TempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var formFile = new FormFile(stream, 0, stream.Length, "file", evt.OriginalFileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = evt.ContentType
        };

        return await fileService.UploadFileAsync(formFile, evt.FolderName, cancellationToken);
    }

    private void DeleteTempFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Geçici dosya silinemedi: {Path}", path);
        }
    }
}
