using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.UpdateReport;

public sealed record UpdateReportCommandResponse(
    Guid Id,
    string Title,
    string Description,
    double? Latitude,
    double? Longitude,
    ReportStatus Status,
    Guid CategoryId,
    string CategoryName,
    bool CategoryAssignedByAi,
    double? AiCategoryConfidence,
    Guid NeighborhoodId,
    ReportPriority Priority,
    IReadOnlyList<UpdateReportMediaResponse> Media,
    DateTime? UpdatedAt);

public sealed record UpdateReportMediaResponse(
    Guid Id,
    string Url,
    string? OriginalFileName,
    string? ContentType,
    long? FileSizeBytes,
    int DisplayOrder,
    MediaType MediaType);
