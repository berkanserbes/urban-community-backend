using Microsoft.AspNetCore.Http;

namespace UrbanCommunity.Application.Interfaces;

public interface IFileService
{
    /// <summary>
    /// Dosyayı sisteme (Local, S3, Azure, Cloudinary vb.) yükler ve döndürülen URL'yi verir.
    /// </summary>
    Task<string> UploadFileAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen URL'deki dosyayı sistemden siler.
    /// </summary>
    Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
