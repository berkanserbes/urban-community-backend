using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.UpdateCommunityPost;

public sealed class UpdateCommunityPostCommandHandler(
    IApplicationDbContext context,
    IFileService fileService)
    : IRequestHandler<UpdateCommunityPostCommand, Result<UpdateCommunityPostCommandResponse>>
{
    private const int MAX_MEDIA_COUNT = 10;

    public async Task<Result<UpdateCommunityPostCommandResponse>> Handle(UpdateCommunityPostCommand request, CancellationToken cancellationToken)
    {
        var post = await context.CommunityPosts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPost), request.Id);

        if (post.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu gönderiyi yalnızca sahibi düzenleyebilir.");

        if (post.Status != CommunityPostStatus.Active)
            return Result<UpdateCommunityPostCommandResponse>.Failure(["Yalnızca aktif topluluk gönderileri düzenlenebilir."]);

        post.Title = request.Title.Trim();
        post.Content = request.Content.Trim();

        var updateMediaResult = await UpdateMediaAsync(post, request.DeleteMediaIds ?? [], request.Media, cancellationToken);

        if (!updateMediaResult.IsSuccess)
            return Result<UpdateCommunityPostCommandResponse>.Failure(["Medya güncellenirken bir hata oluştu. Lütfen tekrar deneyin."]);

        await context.SaveChangesAsync(cancellationToken);
        
        foreach (var url in updateMediaResult.DeletedUrls)
        {
            await fileService.DeleteFileAsync(url, cancellationToken);   
        }

         var media = post.Media
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new UpdateCommunityPostMediaResponse(
                m.Id,
                m.Url,
                m.OriginalFileName,
                m.ContentType,
                m.FileSizeBytes,
                m.DisplayOrder,
                m.MediaType))
            .ToList();

        return Result<UpdateCommunityPostCommandResponse>.Success(
            new UpdateCommunityPostCommandResponse(
                post.Id,
                post.Title,
                post.Content,
                media,
                post.UpdatedAt));
    }

    private async Task<UpdateMediaResult> UpdateMediaAsync(
        CommunityPost post,
        IReadOnlyList<Guid> deleteMediaIds,
        IReadOnlyList<IFormFile>? newMedia,
        CancellationToken cancellationToken)
    {
       var existingMedia = post.Media.ToList();

       var invalidDeleteIds = deleteMediaIds
            .Where(id => existingMedia.All(m => m.Id != id))
            .ToList();

        if(invalidDeleteIds.Count > 0)
            return new UpdateMediaResult(false, Array.Empty<string>());
        
        var replacementMedia = newMedia ?? [];
        var remainingCount = existingMedia.Count - deleteMediaIds.Count;

        if (remainingCount + replacementMedia.Count > MAX_MEDIA_COUNT)
            return new UpdateMediaResult(false, []);

        var mediaToDelete = existingMedia
            .Where(m => deleteMediaIds.Contains(m.Id))
            .ToList();

        var deletedUrls = mediaToDelete.Select(m => m.Url).ToList();

        // Calculate display order for remaining media
        var keptMedia = existingMedia
            .Where(m => !deleteMediaIds.Contains(m.Id))
            .OrderBy(m => m.DisplayOrder)
            .ToList();

        for (var i = 0; i < keptMedia.Count; i++)
        {
            keptMedia[i].DisplayOrder = i;
        }

        //Replace media - if any upload fails, previously uploaded files will be deleted to rollback to original state
        var uploadedMedia = new List<CommunityPostMedia>();
        var uploadedUrls = new List<string>();

        for (var i = 0; i < replacementMedia.Count; i++)
        {
            var file = replacementMedia[i];
            var url = await fileService.UploadFileAsync(file, "community-posts", cancellationToken);

            if (string.IsNullOrWhiteSpace(url))
            {
                foreach (var uploadedUrl in uploadedUrls)
                {   
                    await fileService.DeleteFileAsync(uploadedUrl, cancellationToken);   
                }

                return new UpdateMediaResult(false, []);
            }

            uploadedUrls.Add(url);
            uploadedMedia.Add(new CommunityPostMedia
            {
                CommunityPostId = post.Id,
                Url = url,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                DisplayOrder = keptMedia.Count + i,
                MediaType = ResolveMediaType(file)
            });
        }

        foreach (var item in mediaToDelete)
        {
            context.CommunityPostMedias.Remove(item);
        }

        foreach (var item in uploadedMedia)
        {
            post.Media.Add(item);
        }

        return new UpdateMediaResult(true, deletedUrls);
    }

    private static MediaType ResolveMediaType(IFormFile file)
    {
        return file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
            ? MediaType.Video
            : MediaType.Image;
    }

    private sealed record UpdateMediaResult(bool IsSuccess, IReadOnlyList<string> DeletedUrls);
}
