using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeactivateAllAnnouncements;

public sealed class DeactivateAllAnnouncementsCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeactivateAllAnnouncementsCommand, Result<DeactivateAllAnnouncementsCommandResponse>>
{
    public async Task<Result<DeactivateAllAnnouncementsCommandResponse>> Handle(DeactivateAllAnnouncementsCommand request, CancellationToken cancellationToken)
    {
        var announcements = await context.Announcements
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        if (announcements.Count == 0)
            return Result<DeactivateAllAnnouncementsCommandResponse>.Failure(["Pasif hale getirilecek duyuru bulunamadı."]);

        foreach (var announcement in announcements)
            announcement.Deactivate();

        await context.SaveChangesAsync(cancellationToken);

        return Result<DeactivateAllAnnouncementsCommandResponse>.Success(
            new DeactivateAllAnnouncementsCommandResponse(
                "Tüm duyurular başarıyla pasif hale getirildi.",
                announcements.Count));
    }
}
