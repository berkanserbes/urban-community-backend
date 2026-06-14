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
    Guid CategoryId,
    string CategoryName,
    Guid NeighborhoodId,
    string NeighborhoodName,
    Guid? AssignedDepartmentId,
    string? AssignedDepartmentName,
    DateTime CreatedAt,
    DateTime? ResolvedAt);
