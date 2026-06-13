using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Services;

public sealed class AnnouncementAuthorizationService(IApplicationDbContext context)
    : IAnnouncementAuthorizationService
{
    public async Task EnsureCanCreateAsync(
        Guid requestingUserId,
        AnnouncementScope scope,
        Guid? provinceId,
        Guid? districtId,
        Guid? neighborhoodId,
        CancellationToken cancellationToken)
    {
        var user = await GetActiveUserAsync(requestingUserId, cancellationToken);

        if (user.Role == UserRole.Admin)
            return;

        EnsureOfficial(user);

        if (scope == AnnouncementScope.All)
            throw new ForbiddenAccessException("Tum kullanicilara duyuru yayinlama yetkisi yalnizca adminlere aittir.");

        if (!await IsWithinJurisdictionAsync(
                user,
                scope,
                provinceId,
                districtId,
                neighborhoodId,
                cancellationToken))
        {
            throw new ForbiddenAccessException("Bu lokasyon kapsaminda duyuru yayinlama yetkiniz yok.");
        }
    }

    public async Task EnsureCanManageAsync(
        Guid requestingUserId,
        Announcement announcement,
        CancellationToken cancellationToken)
    {
        var user = await GetActiveUserAsync(requestingUserId, cancellationToken);

        if (user.Role == UserRole.Admin)
            return;

        EnsureOfficial(user);

        if (announcement.UserId != user.Id)
            throw new ForbiddenAccessException("Yalnizca kendi olusturdugunuz duyurulari yonetebilirsiniz.");

        if (!await IsWithinJurisdictionAsync(
                user,
                announcement.Scope,
                announcement.ProvinceId,
                announcement.DistrictId,
                announcement.NeighborhoodId,
                cancellationToken))
        {
            throw new ForbiddenAccessException("Duyuru mevcut yetki alaniniz disinda.");
        }
    }

    public async Task EnsureIsAdminAsync(
        Guid requestingUserId,
        CancellationToken cancellationToken)
    {
        var user = await GetActiveUserAsync(requestingUserId, cancellationToken);

        if (user.Role != UserRole.Admin)
            throw new ForbiddenAccessException("Bu islem yalnizca adminler tarafindan yapilabilir.");
    }

    private async Task<User> GetActiveUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId && user.IsActive, cancellationToken)
            ?? throw new ForbiddenAccessException("Aktif kullanici kaydi bulunamadi.");
    }

    private static void EnsureOfficial(User user)
    {
        if (user.Role != UserRole.Official ||
            !user.OfficialJurisdictionLevel.HasValue)
        {
            throw new ForbiddenAccessException("Duyuru yonetimi icin gecerli bir rol veya yetki alani bulunamadi.");
        }
    }

    private async Task<bool> IsWithinJurisdictionAsync(
        User user,
        AnnouncementScope scope,
        Guid? provinceId,
        Guid? districtId,
        Guid? neighborhoodId,
        CancellationToken cancellationToken)
    {
        return scope switch
        {
            AnnouncementScope.Province when provinceId.HasValue =>
                user.OfficialJurisdictionLevel == OfficialJurisdictionLevel.Province &&
                user.OfficialProvinceId == provinceId,

            AnnouncementScope.District when districtId.HasValue =>
                await CanTargetDistrictAsync(user, districtId.Value, cancellationToken),

            AnnouncementScope.Neighborhood when neighborhoodId.HasValue =>
                await CanTargetNeighborhoodAsync(user, neighborhoodId.Value, cancellationToken),

            _ => false
        };
    }

    private async Task<bool> CanTargetDistrictAsync(
        User user,
        Guid districtId,
        CancellationToken cancellationToken)
    {
        return user.OfficialJurisdictionLevel switch
        {
            OfficialJurisdictionLevel.Province =>
                await context.Districts.AsNoTracking().AnyAsync(
                    district => district.Id == districtId &&
                                district.ProvinceId == user.OfficialProvinceId,
                    cancellationToken),

            OfficialJurisdictionLevel.District =>
                user.OfficialDistrictId == districtId,

            _ => false
        };
    }

    private async Task<bool> CanTargetNeighborhoodAsync(
        User user,
        Guid neighborhoodId,
        CancellationToken cancellationToken)
    {
        return user.OfficialJurisdictionLevel switch
        {
            OfficialJurisdictionLevel.Province =>
                await context.Neighborhoods.AsNoTracking().AnyAsync(
                    neighborhood =>
                        neighborhood.Id == neighborhoodId &&
                        neighborhood.DistrictNavigation != null &&
                        neighborhood.DistrictNavigation.ProvinceId == user.OfficialProvinceId,
                    cancellationToken),

            OfficialJurisdictionLevel.District =>
                await context.Neighborhoods.AsNoTracking().AnyAsync(
                    neighborhood =>
                        neighborhood.Id == neighborhoodId &&
                        neighborhood.DistrictId == user.OfficialDistrictId,
                    cancellationToken),

            OfficialJurisdictionLevel.Neighborhood =>
                user.OfficialNeighborhoodId == neighborhoodId,

            _ => false
        };
    }
}
