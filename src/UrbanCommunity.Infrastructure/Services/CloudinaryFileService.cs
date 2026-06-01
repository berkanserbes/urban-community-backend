using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Services;

public sealed class CloudinaryFileService : IFileService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryFileService()
    {
        var account = new Account(
            GetRequiredEnvVar("CLOUDINARY_CLOUD_NAME"),
            GetRequiredEnvVar("CLOUDINARY_API_KEY"),
            GetRequiredEnvVar("CLOUDINARY_API_SECRET"));

        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return string.Empty;

        await using var stream = file.OpenReadStream();
        var fileDesc = new FileDescription(file.FileName, stream);
        var folder = $"UrbanCommunity/{folderName}";

        if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return await UploadAsync(new ImageUploadParams { File = fileDesc, Folder = folder, UseFilename = false, UniqueFilename = true, Overwrite = false }, cancellationToken);

        if (file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return await UploadAsync(new VideoUploadParams { File = fileDesc, Folder = folder, UseFilename = false, UniqueFilename = true, Overwrite = false }, cancellationToken);

        // PDF vb. ham dosyalar
        return await UploadAsync(new RawUploadParams { File = fileDesc, Folder = folder, UseFilename = false, UniqueFilename = true, Overwrite = false });
    }

    public async Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return false;

        var (publicId, resourceType) = ExtractPublicId(fileUrl);
        if (string.IsNullOrEmpty(publicId))
            return false;

        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId) { ResourceType = resourceType });
        return result.Result == "ok";
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<string> UploadAsync(ImageUploadParams p, CancellationToken ct)
    {
        var result = await _cloudinary.UploadAsync(p, ct);
        if (result.Error != null)
            throw new InvalidOperationException($"Cloudinary image upload failed: {result.Error.Message}");
        return result.SecureUrl?.ToString() ?? throw new InvalidOperationException("Cloudinary returned no URL.");
    }

    private async Task<string> UploadAsync(VideoUploadParams p, CancellationToken ct)
    {
        var result = await _cloudinary.UploadAsync(p, ct);
        if (result.Error != null)
            throw new InvalidOperationException($"Cloudinary video upload failed: {result.Error.Message}");
        return result.SecureUrl?.ToString() ?? throw new InvalidOperationException("Cloudinary returned no URL.");
    }

    private async Task<string> UploadAsync(RawUploadParams p)
        => (await _cloudinary.UploadAsync(p)).SecureUrl?.ToString() ?? string.Empty;

    // URL örneği: https://res.cloudinary.com/{cloud}/{type}/upload/v{ver}/{public_id}.{ext}
    private static (string PublicId, ResourceType ResourceType) ExtractPublicId(string fileUrl)
    {
        try
        {
            var path = new Uri(fileUrl).AbsolutePath;

            var resourceType = ResourceType.Image;
            if (path.Contains("/video/upload/", StringComparison.Ordinal))       resourceType = ResourceType.Video;
            else if (path.Contains("/raw/upload/", StringComparison.Ordinal))    resourceType = ResourceType.Raw;

            const string marker = "/upload/";
            var idx = path.IndexOf(marker, StringComparison.Ordinal);
            if (idx < 0)
                return (string.Empty, resourceType);

            var after = path[(idx + marker.Length)..];

            // İsteğe bağlı versiyon öneki varsa kaldır: v1234567890/
            var slash = after.IndexOf('/');
            if (slash > 1 && after[0] == 'v' && after[1..slash].All(char.IsDigit))
                after = after[(slash + 1)..];

            var dot = after.LastIndexOf('.');
            return (dot >= 0 ? after[..dot] : after, resourceType);
        }
        catch
        {
            return (string.Empty, ResourceType.Image);
        }
    }

    private static string GetRequiredEnvVar(string key) =>
        Environment.GetEnvironmentVariable(key)
        ?? throw new InvalidOperationException($"'{key}' ortam değişkeni tanımlı değil.");
}


