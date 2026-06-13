using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.DeleteCommunityPost;

public sealed class DeleteCommunityPostCommandHandler(
    IApplicationDbContext context,
    IFileService fileService,
    ILogger<DeleteCommunityPostCommandHandler> logger)
    : IRequestHandler<DeleteCommunityPostCommand, Result<DeleteCommunityPostCommandResponse>>
{
    public async Task<Result<DeleteCommunityPostCommandResponse>> Handle(
        DeleteCommunityPostCommand request,
        CancellationToken cancellationToken)
    {
        var post = await context.CommunityPosts
            .Include(item => item.Media)
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPost), request.Id);

        if (!request.IsAdmin && post.UserId != request.RequestingUserId)
        {
            throw new ForbiddenAccessException(
                "Bu gönderiyi yalnızca sahibi veya admin silebilir.");
        }

        var mediaUrls = post.Media.Select(media => media.Url).ToList();
        context.CommunityPosts.Remove(post);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var url in mediaUrls)
        {
            try
            {
                if (!await fileService.DeleteFileAsync(url, CancellationToken.None))
                {
                    logger.LogWarning(
                        "Deleted community post media file could not be removed: {Url}",
                        url);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Deleted community post media file removal failed: {Url}",
                    url);
            }
        }

        return Result<DeleteCommunityPostCommandResponse>.Success(
            new DeleteCommunityPostCommandResponse(
                "Gönderi başarıyla silindi."));
    }
}
