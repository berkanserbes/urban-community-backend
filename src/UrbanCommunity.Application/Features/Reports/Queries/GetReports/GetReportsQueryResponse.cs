using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReports;

public sealed record GetReportsQueryResponse(
    Guid Id,
    string Title,
    string Description,
    ReportStatus Status,
    ReportPriority Priority,
    double? Latitude,
    double? Longitude,
    Guid UserId,
    string ReporterName,
    Guid CategoryId,
    string CategoryName,
    Guid NeighborhoodId,
    string NeighborhoodName,
    Guid? AssignedOrganizationDepartmentId,
    Guid? AssignedOrganizationId,
    string? AssignedOrganizationName,
    Guid? AssignedDepartmentId,
    string? AssignedDepartmentName,
    string? AssignedDepartmentContactEmail,
    string? AssignedDepartmentContactPhone,
    bool AssignmentNeedsReview,
    DateTime CreatedAt,
    DateTime? ResolvedAt);
