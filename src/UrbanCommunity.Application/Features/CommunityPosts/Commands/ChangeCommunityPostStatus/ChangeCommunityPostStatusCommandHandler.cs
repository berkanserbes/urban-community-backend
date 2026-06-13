using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.CommunityPosts.Commands.ChangeCommunityPostStatus;

public sealed class ChangeCommunityPostStatusCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ChangeCommunityPostStatusCommand, Result<ChangeCommunityPostStatusCommandResponse>>
{
    public async Task<Result<ChangeCommunityPostStatusCommandResponse>> Handle(
        ChangeCommunityPostStatusCommand request,
        CancellationToken cancellationToken)
    {
        var post = await context.CommunityPosts
            .FirstOrDefaultAsync(p => p.Id == request.CommunityPostId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommunityPost), request.CommunityPostId);

        if (!request.IsAdmin && post.UserId != request.RequestingUserId)
        {
            throw new ForbiddenAccessException(
                "Topluluk gönderisinin durumunu yalnızca sahibi veya admin değiştirebilir.");
        }

        if (!post.CanTransitionTo(request.NewStatus))
        {
            var allowed = string.Join(", ", post.GetAllowedNextStatuses());
            return Result<ChangeCommunityPostStatusCommandResponse>.Failure(
                [$"Geçersiz durum geçişi: {post.Status} -> {request.NewStatus}. İzin verilen durumlar: {allowed}."]);
        }

        post.ChangeStatus(request.NewStatus);
        await context.SaveChangesAsync(cancellationToken);

        return Result<ChangeCommunityPostStatusCommandResponse>.Success(
            new ChangeCommunityPostStatusCommandResponse(
                post.Id,
                post.Status,
                post.GetAllowedNextStatuses().ToList()));
    }
}

