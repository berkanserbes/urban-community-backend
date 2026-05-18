using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Announcements.Commands.ActivateAnnouncement;

public sealed class ActivateAnnouncementCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ActivateAnnouncementCommand, Result<ActivateAnnouncementCommandResponse>>
{
    public async Task<Result<ActivateAnnouncementCommandResponse>> Handle(ActivateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await context.Announcements
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Announcement), request.Id);

        if (announcement.IsActive)
            return Result<ActivateAnnouncementCommandResponse>.Failure(["Duyuru zaten aktif."]);

        announcement.Activate(request.ExpiresAt);
        await context.SaveChangesAsync(cancellationToken);

        return Result<ActivateAnnouncementCommandResponse>.Success(
            new ActivateAnnouncementCommandResponse(
                announcement.Id,
                "Duyuru başarıyla aktif edildi."));
    }
}
