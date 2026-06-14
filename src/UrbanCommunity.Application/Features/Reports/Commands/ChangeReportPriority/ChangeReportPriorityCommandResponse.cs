using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.ChangeReportPriority;

public sealed record ChangeReportPriorityCommandResponse(
    Guid ReportId,
    ReportPriority Priority,
    DateTime? UpdatedAt);
