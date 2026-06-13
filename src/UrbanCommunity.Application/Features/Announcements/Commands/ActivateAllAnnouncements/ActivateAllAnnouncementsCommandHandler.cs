using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Announcements.Commands.ActivateAllAnnouncements;

public sealed class ActivateAllAnnouncementsCommandHandler(
    IApplicationDbContext context,
    IAnnouncementAuthorizationService authorizationService)
    : IRequestHandler<ActivateAllAnnouncementsCommand, Result<ActivateAllAnnouncementsCommandResponse>>
{
    public async Task<Result<ActivateAllAnnouncementsCommandResponse>> Handle(ActivateAllAnnouncementsCommand request, CancellationToken cancellationToken)
    {
        await authorizationService.EnsureIsAdminAsync(
            request.RequestingUserId,
            cancellationToken);

        var announcements = await context.Announcements
            .Where(a => !a.IsActive)
            .ToListAsync(cancellationToken);

        if (announcements.Count == 0)
            return Result<ActivateAllAnnouncementsCommandResponse>.Failure(["Aktif edilecek duyuru bulunamadı."]);

        foreach (var announcement in announcements)
            announcement.Activate(request.ExpiresAt);

        await context.SaveChangesAsync(cancellationToken);

        return Result<ActivateAllAnnouncementsCommandResponse>.Success(
            new ActivateAllAnnouncementsCommandResponse(
                "Tüm duyurular başarıyla aktif edildi.",
                announcements.Count));
    }
}
