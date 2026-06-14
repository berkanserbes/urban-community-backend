using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.CreateReport;

public sealed record CreateReportCommandResponse(
    Guid Id,
    string Title,
    ReportStatus Status,
    ReportPriority Priority,
    double? Latitude,
    double? Longitude,
    Guid CategoryId,
    string CategoryName,
    bool CategoryAssignedByAi,
    double? AiCategoryConfidence,
    Guid NeighborhoodId,
    IReadOnlyList<CreateReportMediaResponse> Media,
    DateTime CreatedAt);

public sealed record CreateReportMediaResponse(
    Guid Id,
    string Url,
    string? OriginalFileName,
    string? ContentType,
    long? FileSizeBytes,
    int DisplayOrder,
    MediaType MediaType);
