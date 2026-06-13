using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.CreateCommunityPost;

public sealed class CreateCommunityPostCommandHandler(
    IApplicationDbContext context,
    IFileService fileService,
    ILogger<CreateCommunityPostCommandHandler> logger)
    : IRequestHandler<CreateCommunityPostCommand, Result<CreateCommunityPostCommandResponse>>
{
    public async Task<Result<CreateCommunityPostCommandResponse>> Handle(
        CreateCommunityPostCommand request,
        CancellationToken cancellationToken)
    {
        var accessScope = await CommunityPostAccessPolicy.ResolveUserAccessScopeAsync(
            context,
            request.UserId,
            cancellationToken);

        await EnsureLocationExistsAsync(request, cancellationToken);

        if (!CommunityPostAccessPolicy.CanCreateInScope(
                request.Scope,
                request.ProvinceId,
                request.DistrictId,
                request.NeighborhoodId,
                accessScope))
        {
            throw new ForbiddenAccessException(
                "Bu kapsamda topluluk gönderisi oluşturma yetkiniz yok.");
        }

        var post = new CommunityPost
        {
            Type = request.Type,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            UserId = request.UserId,
            Scope = request.Scope,
            ProvinceId = request.Scope == CommunityPostScope.Province
                ? request.ProvinceId
                : null,
            DistrictId = request.Scope == CommunityPostScope.District
                ? request.DistrictId
                : null,
            NeighborhoodId = request.Scope == CommunityPostScope.Neighborhood
                ? request.NeighborhoodId
                : null
        };

        try
        {
            if (!await UploadMediaAsync(post, request.Media, cancellationToken))
            {
                await DeleteFilesSafelyAsync(
                    post.Media.Select(media => media.Url));
                return Result<CreateCommunityPostCommandResponse>.Failure(
                    ["Medya dosyası yüklenemedi. Lütfen tekrar deneyin."]);
            }

            context.CommunityPosts.Add(post);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await DeleteFilesSafelyAsync(
                post.Media.Select(media => media.Url));
            throw;
        }

        var media = post.Media
            .OrderBy(item => item.DisplayOrder)
            .Select(item => new CreateCommunityPostMediaResponse(
                item.Id,
                item.Url,
                item.OriginalFileName,
                item.ContentType,
                item.FileSizeBytes,
                item.DisplayOrder,
                item.MediaType))
            .ToList();

        return Result<CreateCommunityPostCommandResponse>.Success(
            new CreateCommunityPostCommandResponse(
                post.Id,
                post.Type.ToString(),
                post.Title,
                post.Status.ToString(),
                post.Scope,
                post.ProvinceId,
                post.DistrictId,
                post.NeighborhoodId,
                media,
                post.CreatedAt));
    }

    private async Task<bool> UploadMediaAsync(
        CommunityPost post,
        IReadOnlyList<IFormFile>? media,
        CancellationToken cancellationToken)
    {
        if (media is not { Count: > 0 })
            return true;

        for (var index = 0; index < media.Count; index++)
        {
            var file = media[index];
            var url = await fileService.UploadFileAsync(
                file,
                "community-posts",
                cancellationToken);

            if (string.IsNullOrWhiteSpace(url))
                return false;

            post.Media.Add(new CommunityPostMedia
            {
                CommunityPostId = post.Id,
                Url = url,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                DisplayOrder = index,
                MediaType = ResolveMediaType(file)
            });
        }

        return true;
    }

    private async Task DeleteFilesSafelyAsync(
        IEnumerable<string> urls)
    {
        foreach (var url in urls)
        {
            try
            {
                if (!await fileService.DeleteFileAsync(url, CancellationToken.None))
                {
                    logger.LogWarning(
                        "Community post media rollback could not delete file: {Url}",
                        url);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Community post media rollback failed for file: {Url}",
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

    private async Task EnsureLocationExistsAsync(
        CreateCommunityPostCommand request,
        CancellationToken cancellationToken)
    {
        var locationExists = request.Scope switch
        {
            CommunityPostScope.Province => request.ProvinceId.HasValue &&
                await context.Provinces.AsNoTracking().AnyAsync(
                    province => province.Id == request.ProvinceId.Value,
                    cancellationToken),

            CommunityPostScope.District => request.DistrictId.HasValue &&
                await context.Districts.AsNoTracking().AnyAsync(
                    district => district.Id == request.DistrictId.Value,
                    cancellationToken),

            CommunityPostScope.Neighborhood => request.NeighborhoodId.HasValue &&
                await context.Neighborhoods.AsNoTracking().AnyAsync(
                    neighborhood => neighborhood.Id == request.NeighborhoodId.Value,
                    cancellationToken),

            _ => false
        };

        if (locationExists)
            return;

        throw request.Scope switch
        {
            CommunityPostScope.Province =>
                new NotFoundException(nameof(Province), request.ProvinceId ?? Guid.Empty),
            CommunityPostScope.District =>
                new NotFoundException(nameof(District), request.DistrictId ?? Guid.Empty),
            _ => new NotFoundException(
                nameof(Neighborhood),
                request.NeighborhoodId ?? Guid.Empty)
        };
    }
}
