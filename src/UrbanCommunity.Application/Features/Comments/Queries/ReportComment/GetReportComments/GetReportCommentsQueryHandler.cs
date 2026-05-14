using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Comments.Queries.ReportComment.GetReportComments;

public sealed class GetReportCommentsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReportCommentsQuery, Result<IReadOnlyList<GetReportCommentsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetReportCommentsQueryResponse>>> Handle(GetReportCommentsQuery request, CancellationToken cancellationToken)
    {
        if (!await context.Reports.AsNoTracking().AnyAsync(report => report.Id == request.ReportId, cancellationToken))
        {
            throw new NotFoundException(nameof(Report), request.ReportId);
        }

        var comments = await context.ReportComments
            .AsNoTracking()
            .Where(comment => comment.ReportId == request.ReportId)
            .Include(comment => comment.User)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(cancellationToken);

        IReadOnlyList<GetReportCommentsQueryResponse> result = comments
            .Select(comment => new GetReportCommentsQueryResponse(
                comment.Id,
                comment.UserId,
                comment.User.FullName,
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt))
            .ToList();

        return Result<IReadOnlyList<GetReportCommentsQueryResponse>>.Success(result);
    }
}
