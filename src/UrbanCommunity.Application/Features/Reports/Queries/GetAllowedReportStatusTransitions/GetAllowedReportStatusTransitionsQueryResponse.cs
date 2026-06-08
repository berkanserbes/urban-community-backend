using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetAllowedReportStatusTransitions;

public sealed record GetAllowedReportStatusTransitionsQueryResponse(
    Guid ReportId,
    ReportStatus CurrentStatus,
    IReadOnlyList<ReportStatus> AllowedNextStatuses);
