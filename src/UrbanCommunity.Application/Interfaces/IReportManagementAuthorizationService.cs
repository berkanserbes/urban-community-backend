using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Interfaces;

public interface IReportManagementAuthorizationService
{
    Task<ReportManagementAccess> GetAccessAsync(
        Guid requestingUserId,
        bool isAdmin,
        CancellationToken cancellationToken);

    IQueryable<Report> ApplyManageableScope(
        IQueryable<Report> reports,
        ReportManagementAccess access);

    Task EnsureCanManageAsync(
        Guid reportId,
        Guid requestingUserId,
        bool isAdmin,
        CancellationToken cancellationToken);
}

public sealed record ReportManagementAccess(
    bool IsAdmin,
    Guid RequestingUserId,
    Guid? OrganizationDepartmentId,
    OfficialJurisdictionLevel? JurisdictionLevel,
    Guid? JurisdictionId);
