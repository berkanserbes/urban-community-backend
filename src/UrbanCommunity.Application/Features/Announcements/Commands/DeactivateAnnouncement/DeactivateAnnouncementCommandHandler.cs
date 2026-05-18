using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeactivateAnnouncement;

public sealed class DeactivateAnnouncementCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateAnnouncementCommand, Result<DeactivateAnnouncementCommandResponse>>
{
    public async Task<Result<DeactivateAnnouncementCommandResponse>> Handle(DeactivateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await context.Announcements
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Announcement), request.Id);

        announcement.Deactivate();
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeactivateAnnouncementCommandResponse>.Success(
            new DeactivateAnnouncementCommandResponse(
                announcement.Id,
                "Duyuru başarıyla pasif hale getirildi."));
    }
}
