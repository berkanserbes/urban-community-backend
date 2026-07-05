using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.UpdateCommunityPost;

public sealed class UpdateCommunityPostCommandHandler(
    IApplicationDbContext context,
    IFileService fileService,
    ILogger<UpdateCommunityPostCommandHandler> logger)
    : IRequestHandler<UpdateCommunityPostCommand, Result<UpdateCommunityPostCommandResponse>>
{
    private const int MaxMediaCount = 10;

    public async Task<Result<UpdateCommunityPostCommandResponse>> Handle(
        UpdateCommunityPostCommand request,
        CancellationToken cancellationToken)
    {
        var post = await context.CommunityPosts
            .Include(post => post.Media)
            .FirstOrDefaultAsync(post => post.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPost), request.Id);

        if (post.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu gönderiyi yalnızca sahibi düzenleyebilir.");

        if (post.Status != CommunityPostStatus.Active)
        {
            return Result<UpdateCommunityPostCommandResponse>.Failure(
                ["Yalnızca aktif topluluk gönderileri düzenlenebilir."]);
        }

        var mediaUpdate = await PrepareMediaUpdateAsync(post, request, cancellationToken);
        if (!mediaUpdate.IsSuccess)
        {
            return Result<UpdateCommunityPostCommandResponse>.Failure(
                [mediaUpdate.ErrorMessage!]);
        }

        post.Title = request.Title.Trim();
        post.Content = request.Content.Trim();

        foreach (var media in mediaUpdate.MediaToDelete)
            context.CommunityPostMedias.Remove(media);

        foreach (var media in mediaUpdate.UploadedMedia)
            post.Media.Add(media);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await DeleteFilesSafelyAsync(
                mediaUpdate.UploadedMedia.Select(media => media.Url),
                "veritabanı güncellemesi başarısız olduğu için");
            throw;
        }

        await DeleteFilesSafelyAsync(
            mediaUpdate.MediaToDelete.Select(media => media.Url),
            "medya kaydı silindiği için");

        var mediaResponse = post.Media
            .Except(mediaUpdate.MediaToDelete)
            .OrderBy(media => media.DisplayOrder)
            .Select(media => new UpdateCommunityPostMediaResponse(
                media.Id,
                media.Url,
                media.OriginalFileName,
                media.ContentType,
                media.FileSizeBytes,
                media.DisplayOrder,
                media.MediaType))
            .ToList();

        return Result<UpdateCommunityPostCommandResponse>.Success(
            new UpdateCommunityPostCommandResponse(
                post.Id,
                post.Title,
                post.Content,
                mediaResponse,
                post.UpdatedAt));
    }

    private async Task<MediaUpdatePreparation> PrepareMediaUpdateAsync(
        CommunityPost post,
        UpdateCommunityPostCommand request,
        CancellationToken cancellationToken)
    {
        var existingMedia = post.Media.ToList();
        var keepIds = request.KeepMediaIds;

        if (keepIds is not null &&
            keepIds.Any(id => existingMedia.All(media => media.Id != id)))
        {
            return MediaUpdatePreparation.Failure(
                "Korunacak medya ID'lerinden en az biri bu gönderiye ait değil.");
        }

        var keptMedia = keepIds is null
            ? existingMedia
            : existingMedia.Where(media => keepIds.Contains(media.Id)).ToList();

        var replacementMedia = request.Media ?? [];
        if (keptMedia.Count + replacementMedia.Count > MaxMediaCount)
        {
            return MediaUpdatePreparation.Failure(
                $"Bir gönderide en fazla {MaxMediaCount} medya bulunabilir.");
        }

        keptMedia = keptMedia.OrderBy(media => media.DisplayOrder).ToList();
        for (var index = 0; index < keptMedia.Count; index++)
            keptMedia[index].DisplayOrder = index;

        var uploadedMedia = new List<CommunityPostMedia>();

        try
        {
            for (var index = 0; index < replacementMedia.Count; index++)
            {
                var file = replacementMedia[index];
                string url;

                try
                {
                    url = await fileService.UploadFileAsync(
                        file,
                        "community-posts",
                        cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch
                {
                    await DeleteFilesSafelyAsync(
                        uploadedMedia.Select(media => media.Url),
                        "medya yüklemesi hata verdiği için");

                    return MediaUpdatePreparation.Failure(
                        "Medya dosyası yüklenemedi. Lütfen tekrar deneyin.");
                }

                if (string.IsNullOrWhiteSpace(url))
                {
                    await DeleteFilesSafelyAsync(
                        uploadedMedia.Select(media => media.Url),
                        "medya yüklemesi tamamlanamadığı için");

                    return MediaUpdatePreparation.Failure(
                        "Medya dosyası yüklenemedi. Lütfen tekrar deneyin.");
                }

                uploadedMedia.Add(new CommunityPostMedia
                {
                    CommunityPostId = post.Id,
                    Url = url,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSizeBytes = file.Length,
                    DisplayOrder = keptMedia.Count + index,
                    MediaType = ResolveMediaType(file)
                });
            }
        }
        catch
        {
            await DeleteFilesSafelyAsync(
                uploadedMedia.Select(media => media.Url),
                "medya yüklemesi hata verdiği için");
            throw;
        }

        var mediaToDelete = existingMedia.Except(keptMedia).ToList();
        return MediaUpdatePreparation.Success(mediaToDelete, uploadedMedia);
    }

    private async Task DeleteFilesSafelyAsync(
        IEnumerable<string> urls,
        string reason)
    {
        foreach (var url in urls)
        {
            try
            {
                if (!await fileService.DeleteFileAsync(url, CancellationToken.None))
                {
                    logger.LogWarning(
                        "Community post media file could not be deleted ({Reason}): {Url}",
                        reason,
                        url);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Community post media file deletion failed ({Reason}): {Url}",
                    reason,
                    url);
            }
        }
    }

    private static MediaType ResolveMediaType(IFormFile file)
    {
        return file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
            ? MediaType.Video
            : MediaType.Image;
    }

    private sealed record MediaUpdatePreparation(
        bool IsSuccess,
        string? ErrorMessage,
        IReadOnlyList<CommunityPostMedia> MediaToDelete,
        IReadOnlyList<CommunityPostMedia> UploadedMedia)
    {
        public static MediaUpdatePreparation Failure(string message) =>
            new(false, message, [], []);

        public static MediaUpdatePreparation Success(
            IReadOnlyList<CommunityPostMedia> mediaToDelete,
            IReadOnlyList<CommunityPostMedia> uploadedMedia) =>
            new(true, null, mediaToDelete, uploadedMedia);
    }
}
