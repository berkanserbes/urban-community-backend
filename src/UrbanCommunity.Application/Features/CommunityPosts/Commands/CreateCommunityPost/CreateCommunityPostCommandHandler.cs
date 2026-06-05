using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.CreateCommunityPost;

public sealed class CreateCommunityPostCommandHandler(
    IApplicationDbContext context,
    IFileService fileService)
    : IRequestHandler<CreateCommunityPostCommand, Result<CreateCommunityPostCommandResponse>>
{
    public async Task<Result<CreateCommunityPostCommandResponse>> Handle(CreateCommunityPostCommand request, CancellationToken cancellationToken)
    {
        var accessScope = await CommunityPostAccessPolicy.ResolveUserAccessScopeAsync(context, request.UserId, cancellationToken);

        await EnsureLocationExistsAsync(request, cancellationToken);

        if (!CommunityPostAccessPolicy.CanCreateInScope(
            request.Scope,
            request.ProvinceId,
            request.DistrictId,
            request.NeighborhoodId,
            accessScope))
        {
            throw new ForbiddenAccessException("Bu kapsamda topluluk gönderisi oluşturma yetkiniz yok.");
        }

        var post = new CommunityPost
        {
            Type = request.Type,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            UserId = request.UserId,
            Scope = request.Scope,
            ProvinceId = request.Scope == CommunityPostScope.Province ? request.ProvinceId : null,
            DistrictId = request.Scope == CommunityPostScope.District ? request.DistrictId : null,
            NeighborhoodId = request.Scope == CommunityPostScope.Neighborhood ? request.NeighborhoodId : null
        };

        if (request.Media is { Count: > 0 })
        {
            for (var i = 0; i < request.Media.Count; i++)
            {
                var file = request.Media[i];
                var url = await fileService.UploadFileAsync(file, "community-posts", cancellationToken);

                if (string.IsNullOrWhiteSpace(url))
                    return Result<CreateCommunityPostCommandResponse>.Failure(["Medya dosyası yüklenemedi. Lütfen tekrar deneyin."]);

                post.Media.Add(new CommunityPostMedia
                {
                    CommunityPostId = post.Id,
                    Url = url,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSizeBytes = file.Length,
                    DisplayOrder = i,
                    MediaType = ResolveMediaType(file)
                });
            }
        }

        context.CommunityPosts.Add(post);
        await context.SaveChangesAsync(cancellationToken);

        var media = post.Media
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new CreateCommunityPostMediaResponse(
                m.Id,
                m.Url,
                m.OriginalFileName,
                m.ContentType,
                m.FileSizeBytes,
                m.DisplayOrder,
                m.MediaType))
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

    private static MediaType ResolveMediaType(IFormFile file)
    {
        return file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
            ? MediaType.Video
            : MediaType.Image;
    }

    private async Task EnsureLocationExistsAsync(CreateCommunityPostCommand request, CancellationToken cancellationToken)
    {
        switch (request.Scope)
        {
            case CommunityPostScope.Province:
                if (!request.ProvinceId.HasValue ||
                    !await context.Provinces.AsNoTracking().AnyAsync(province => province.Id == request.ProvinceId.Value, cancellationToken))
                {
                    throw new NotFoundException(nameof(Province), request.ProvinceId ?? Guid.Empty);
                }
                break;

            case CommunityPostScope.District:
                if (!request.DistrictId.HasValue ||
                    !await context.Districts.AsNoTracking().AnyAsync(district => district.Id == request.DistrictId.Value, cancellationToken))
                {
                    throw new NotFoundException(nameof(District), request.DistrictId ?? Guid.Empty);
                }
                break;

            case CommunityPostScope.Neighborhood:
                if (!request.NeighborhoodId.HasValue ||
                    !await context.Neighborhoods.AsNoTracking().AnyAsync(neighborhood => neighborhood.Id == request.NeighborhoodId.Value, cancellationToken))
                {
                    throw new NotFoundException(nameof(Neighborhood), request.NeighborhoodId ?? Guid.Empty);
                }
                break;
        }
    }
}
