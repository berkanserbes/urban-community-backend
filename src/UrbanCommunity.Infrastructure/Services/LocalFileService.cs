using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Services;

public sealed class LocalFileService(IHostEnvironment env) : IFileService
{
    private string UploadsRoot => Path.Combine(env.ContentRootPath, "wwwroot", "uploads");

    public async Task<string> UploadFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return string.Empty;

        var uploadDir = Path.GetFullPath(Path.Combine(UploadsRoot, folderName));
        if (!uploadDir.StartsWith(UploadsRoot, StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/uploads/{folderName}/{fileName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return Task.FromResult(false);

        var filePath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "wwwroot", fileUrl.TrimStart('/')));

        if (!filePath.StartsWith(UploadsRoot, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(false);

        if (!File.Exists(filePath))
            return Task.FromResult(false);

        File.Delete(filePath);
        return Task.FromResult(true);
    }
}

