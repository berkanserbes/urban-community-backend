using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetAllowedReportStatusTransitions;

public sealed class GetAllowedReportStatusTransitionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAllowedReportStatusTransitionsQuery, Result<GetAllowedReportStatusTransitionsQueryResponse>>
{
    public async Task<Result<GetAllowedReportStatusTransitionsQueryResponse>> Handle(
        GetAllowedReportStatusTransitionsQuery request, CancellationToken cancellationToken)
    {
        var report = await context.Reports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), request.ReportId);

        var allowed = report.GetAllowedNextStatuses()
            .Where(status => status != Domain.Enums.ReportStatus.AI_Review)
            .ToList();

        return Result<GetAllowedReportStatusTransitionsQueryResponse>.Success(
            new GetAllowedReportStatusTransitionsQueryResponse(report.Id, report.Status, allowed));
    }
}
