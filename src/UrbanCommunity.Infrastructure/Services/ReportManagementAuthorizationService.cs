using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Services;

public sealed class ReportManagementAuthorizationService(IApplicationDbContext context)
    : IReportManagementAuthorizationService
{
    public async Task<ReportManagementAccess> GetAccessAsync(
        Guid requestingUserId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(user =>
                user.Id == requestingUserId &&
                user.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            throw new ForbiddenAccessException("Aktif kullanıcı kaydı bulunamadı.");

        if (user.Role == UserRole.Admin && isAdmin)
        {
            return new ReportManagementAccess(
                true,
                user.Id,
                null,
                null,
                null);
        }

        if (user.Role != UserRole.Official ||
            isAdmin ||
            !user.DepartmentId.HasValue ||
            !user.OfficialJurisdictionLevel.HasValue)
        {
            throw new ForbiddenAccessException("Rapor yönetimi için geçerli bir rolünüz bulunmuyor.");
        }

        var jurisdictionId = user.OfficialProvinceId ??
            user.OfficialDistrictId ??
            user.OfficialNeighborhoodId;

        if (!jurisdictionId.HasValue)
        {
            throw new ForbiddenAccessException("Rapor yönetimi için geçerli bir Official yetki alanınız bulunmuyor.");
        }

        var access = new ReportManagementAccess(
            false,
            user.Id,
            user.DepartmentId,
            user.OfficialJurisdictionLevel,
            jurisdictionId);

        return access;
    }

    public IQueryable<Report> ApplyManageableScope(
        IQueryable<Report> reports,
        ReportManagementAccess access)
    {
        if (access.IsAdmin)
            return reports;

        var geographicallyScopedReports = access.JurisdictionLevel switch
        {
            OfficialJurisdictionLevel.Province =>
                reports.Where(report =>
                    report.Neighborhood.DistrictNavigation.ProvinceId == access.JurisdictionId),

            OfficialJurisdictionLevel.District =>
                reports.Where(report =>
                    report.Neighborhood.DistrictId == access.JurisdictionId),

            OfficialJurisdictionLevel.Neighborhood =>
                reports.Where(report =>
                    report.NeighborhoodId == access.JurisdictionId),

            _ => reports.Where(_ => false)
        };

        return geographicallyScopedReports.Where(report =>
            !report.AssignedDepartmentId.HasValue ||
            report.AssignedDepartmentId == access.DepartmentId);
    }

    public async Task EnsureCanManageAsync(
        Guid reportId,
        Guid requestingUserId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var reports = context.Reports.AsNoTracking();

        if (!await reports.AnyAsync(report => report.Id == reportId, cancellationToken))
            throw new NotFoundException(nameof(Report), reportId);

        var access = await GetAccessAsync(requestingUserId, isAdmin, cancellationToken);
        var canManage = await ApplyManageableScope(reports, access)
            .AnyAsync(report => report.Id == reportId, cancellationToken);

        if (!canManage)
            throw new ForbiddenAccessException("Bu rapor yetki alanınız veya departmanınız dışında.");
    }
}
