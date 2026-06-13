using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Interfaces;

public interface IAnnouncementAuthorizationService
{
    Task EnsureCanCreateAsync(
        Guid requestingUserId,
        AnnouncementScope scope,
        Guid? provinceId,
        Guid? districtId,
        Guid? neighborhoodId,
        CancellationToken cancellationToken);

    Task EnsureCanManageAsync(
        Guid requestingUserId,
        Announcement announcement,
        CancellationToken cancellationToken);

    Task EnsureIsAdminAsync(
        Guid requestingUserId,
        CancellationToken cancellationToken);
}
