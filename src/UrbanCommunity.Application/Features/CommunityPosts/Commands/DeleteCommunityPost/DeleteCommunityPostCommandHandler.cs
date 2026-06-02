using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.DeleteCommunityPost;

public sealed class DeleteCommunityPostCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCommunityPostCommand, Result<DeleteCommunityPostCommandResponse>>
{
    public async Task<Result<DeleteCommunityPostCommandResponse>> Handle(DeleteCommunityPostCommand request, CancellationToken cancellationToken)
    {
        var post = await context.CommunityPosts
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPost), request.Id);

        if (!request.IsAdmin && post.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu gönderiyi yalnızca sahibi veya admin silebilir.");

        context.CommunityPosts.Remove(post);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteCommunityPostCommandResponse>.Success(
            new DeleteCommunityPostCommandResponse("Gönderi başarıyla silindi."));
    }
}
