using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Reports.Commands.DeleteReport;

public sealed record DeleteReportCommand(
    Guid ReportId,
    Guid RequestingUserId,
    bool IsAdmin) : IRequest<Result<DeleteReportCommandResponse>>;
