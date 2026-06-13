using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.CommunityPosts.Queries.GetCommunityPostById;

public sealed class GetCommunityPostByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCommunityPostByIdQuery, Result<GetCommunityPostByIdQueryResponse>>
{
    public async Task<Result<GetCommunityPostByIdQueryResponse>> Handle(GetCommunityPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await context.CommunityPosts
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Province)
            .Include(p => p.District)
            .Include(p => p.Neighborhood)
            .Include(p => p.Media.OrderBy(m => m.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPost), request.Id);

        if (!request.IsAdmin)
        {
            var accessScope = await CommunityPostAccessPolicy.ResolveUserAccessScopeAsync(context, request.RequestingUserId, cancellationToken);

            if (!post.IsVisibleTo(accessScope))
            {
                throw new ForbiddenAccessException("Bu topluluk gönderisini görüntüleme yetkiniz yok.");
            }
        }

        var media = post.Media
            .Select(m => new CommunityPostMediaDto(m.Id, m.Url, m.ThumbnailUrl, m.ContentType, m.DisplayOrder, m.MediaType))
            .ToList();

        var commentCount = await context.CommunityPostComments
            .AsNoTracking()
            .CountAsync(comment => comment.CommunityPostId == post.Id, cancellationToken);

        var allowedNextStatuses = request.IsAdmin || post.UserId == request.RequestingUserId
            ? post.GetAllowedNextStatuses().ToList()
            : [];

        return Result<GetCommunityPostByIdQueryResponse>.Success(new GetCommunityPostByIdQueryResponse(
            post.Id,
            post.Type,
            post.Title,
            post.Content,
            post.Status,
            post.UserId,
            $"{post.User.FirstName} {post.User.LastName}",
            post.Scope,
            post.ProvinceId,
            post.Province?.Name,
            post.DistrictId,
            post.District?.Name,
            post.NeighborhoodId,
            post.Neighborhood?.Name,
            allowedNextStatuses,
            media,
            commentCount,
            post.CreatedAt,
            post.UpdatedAt));
    }
}
