using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeleteAnnouncement;

public sealed class DeleteAnnouncementCommandHandler(
    IApplicationDbContext context,
    IAnnouncementAuthorizationService authorizationService)
    : IRequestHandler<DeleteAnnouncementCommand, Result<DeleteAnnouncementCommandResponse>>
{
    public async Task<Result<DeleteAnnouncementCommandResponse>> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        await authorizationService.EnsureIsAdminAsync(
            request.RequestingUserId,
            cancellationToken);

        var announcement = await context.Announcements
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Announcement), request.Id);

        context.Announcements.Remove(announcement);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteAnnouncementCommandResponse>.Success(
            new DeleteAnnouncementCommandResponse(
                announcement.Id,
                "Duyuru başarıyla silindi."));
    }
}
