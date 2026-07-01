using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReportById;

public sealed record GetReportByIdQueryResponse(
    Guid Id,
    string Title,
    string Description,
    ReportStatus Status,
    ReportPriority Priority,
    double? Latitude,
    double? Longitude,
    Guid UserId,
    string UserFullName,
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
    IReadOnlyList<ReportMediaDto> Media,
    IReadOnlyList<ReportHistoryDto> History,
    IReadOnlyList<SimilarReportDto> SimilarReports,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ResolvedAt);

public sealed record ReportMediaDto(
    Guid Id,
    string Url,
    string? ThumbnailUrl,
    string? OriginalFileName,
    string? ContentType,
    long? FileSizeBytes,
    int DisplayOrder,
    MediaType MediaType);

public sealed record ReportHistoryDto(
    Guid Id,
    ReportStatus FromStatus,
    ReportStatus ToStatus,
    string? Note,
    Guid? ChangedByUserId,
    string? ChangedByUserFullName,
    DateTime ChangedAt);

public sealed record SimilarReportDto(
    Guid ReportId,
    string Title,
    ReportStatus Status,
    double SimilarityScore,
    string Reasoning);
