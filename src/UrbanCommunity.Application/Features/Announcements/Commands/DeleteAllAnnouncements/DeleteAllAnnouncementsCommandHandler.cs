using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeleteAllAnnouncements;

public sealed class DeleteAllAnnouncementsCommandHandler(
    IApplicationDbContext context,
    IAnnouncementAuthorizationService authorizationService)
    : IRequestHandler<DeleteAllAnnouncementsCommand, Result<DeleteAllAnnouncementsCommandResponse>>
{
    public async Task<Result<DeleteAllAnnouncementsCommandResponse>> Handle(DeleteAllAnnouncementsCommand request, CancellationToken cancellationToken)
    {
        await authorizationService.EnsureIsAdminAsync(
            request.RequestingUserId,
            cancellationToken);

        var announcements = await context.Announcements.ToListAsync(cancellationToken);

        if (announcements.Count == 0)
            return Result<DeleteAllAnnouncementsCommandResponse>.Failure(["Silinecek duyuru bulunamadı."]);

        context.Announcements.RemoveRange(announcements);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteAllAnnouncementsCommandResponse>.Success(
            new DeleteAllAnnouncementsCommandResponse(
                "Tüm duyurular başarıyla silindi.",
                announcements.Count));
    }
}
