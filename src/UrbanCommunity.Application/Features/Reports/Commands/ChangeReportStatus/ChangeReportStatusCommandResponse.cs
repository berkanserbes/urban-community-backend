using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.ChangeReportStatus;

public sealed record ChangeReportStatusCommandResponse(
    Guid ReportId,
    ReportStatus FromStatus,
    ReportStatus ToStatus,
    DateTime ChangedAt,
    string Message);
