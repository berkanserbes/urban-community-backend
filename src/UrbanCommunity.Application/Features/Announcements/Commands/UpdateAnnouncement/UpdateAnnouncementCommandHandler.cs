using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Announcements.Commands.UpdateAnnouncement;

public sealed class UpdateAnnouncementCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateAnnouncementCommand, Result<UpdateAnnouncementCommandResponse>>
{
    public async Task<Result<UpdateAnnouncementCommandResponse>> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await context.Announcements
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Announcement), request.Id);

        announcement.Title = request.Title.Trim();
        announcement.Content = request.Content.Trim();
        announcement.SetExpiration(request.ExpiresAt);

        await context.SaveChangesAsync(cancellationToken);

        return Result<UpdateAnnouncementCommandResponse>.Success(
            new UpdateAnnouncementCommandResponse(
                announcement.Id,
                announcement.Title,
                announcement.Content,
                announcement.ExpiresAt,
                announcement.UpdatedAt ?? announcement.CreatedAt));
    }
}
