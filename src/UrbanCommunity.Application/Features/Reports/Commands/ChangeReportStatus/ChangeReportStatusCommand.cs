using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.ChangeReportStatus;

public sealed record ChangeReportStatusCommand(
    Guid ReportId,
    ReportStatus NewStatus,
    Guid RequestingUserId,
    bool IsAdmin,
    string? Note = null) : IRequest<Result<ChangeReportStatusCommandResponse>>;
