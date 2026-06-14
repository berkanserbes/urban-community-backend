using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.ChangeReportPriority;

public sealed record ChangeReportPriorityCommand(
    Guid ReportId,
    ReportPriority Priority,
    Guid RequestingUserId,
    bool IsAdmin) : IRequest<Result<ChangeReportPriorityCommandResponse>>;
