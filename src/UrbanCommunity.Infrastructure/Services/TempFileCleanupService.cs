using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UrbanCommunity.Infrastructure.Services;

/// <summary>
/// Periyodik olarak temp/uploads klasörünü temizler.
/// Consumer crash vb. nedenlerle süresi dolan geçici dosyaları siler.
/// </summary>
public sealed class TempFileCleanupService : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

    private static readonly Dictionary<string, TimeSpan> ExtensionMaxAge = new()
    {
        // Resim
        { ".jpg",  TimeSpan.FromHours(2) },
        { ".jpeg", TimeSpan.FromHours(2) },
        { ".png",  TimeSpan.FromHours(2) },
        { ".webp", TimeSpan.FromHours(2) },
        { ".gif",  TimeSpan.FromHours(2) },

        // Video
        { ".mp4",  TimeSpan.FromHours(6) },
        { ".mov",  TimeSpan.FromHours(6) },
        { ".avi",  TimeSpan.FromHours(6) },
        { ".webm", TimeSpan.FromHours(6) },

        // Belge
        { ".pdf",  TimeSpan.FromHours(3) },
    };

    private static readonly TimeSpan DefaultMaxAge = TimeSpan.FromHours(2);

    private readonly ILogger<TempFileCleanupService> _logger;
    private readonly string _tempRoot;

    public TempFileCleanupService(
        IHostEnvironment env,
        ILogger<TempFileCleanupService> logger)
    {
        _logger = logger;
        _tempRoot = Path.Combine(env.ContentRootPath, "temp", "uploads");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "TempFileCleanupService başlatıldı. Temp klasör: {TempRoot}", _tempRoot);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanupOldTempFilesAsync(stoppingToken);
                await Task.Delay(CleanupInterval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown, normal akış
        }
    }

    private async Task CleanupOldTempFilesAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_tempRoot))
        {
            _logger.LogDebug(
                "Temp klasör bulunamadı, atlanıyor: {TempRoot}", _tempRoot);
            return;
        }

        var cleanedCount = 0;

        foreach (var file in Directory.EnumerateFiles(_tempRoot))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                var maxAge = ExtensionMaxAge.GetValueOrDefault(ext, DefaultMaxAge);
                var cutoff = DateTime.UtcNow - maxAge;
                var lastWrite = File.GetLastWriteTimeUtc(file);

                if (lastWrite >= cutoff)
                    continue;

                await Task.Run(() => File.Delete(file), cancellationToken);
                cleanedCount++;

                _logger.LogDebug(
                    "Süresi dolan geçici dosya temizlendi: {Path} (son yazma: {LastWrite:O})",
                    file, lastWrite);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Temp dosya silinemedi: {Path}", file);
            }
        }

        if (cleanedCount > 0)
        {
            _logger.LogInformation(
                "{Count} adet süresi dolan geçici dosya temizlendi.", cleanedCount);
        }
    }
}