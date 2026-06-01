using Microsoft.AspNetCore.Http;

namespace UrbanCommunity.Application.Interfaces;

public interface IBackgroundFileUploadService
{
    Task<Guid> EnqueueAsync(
        IFormFile file,
        string folderName,
        Guid userId,
        CancellationToken cancellationToken = default);
}
