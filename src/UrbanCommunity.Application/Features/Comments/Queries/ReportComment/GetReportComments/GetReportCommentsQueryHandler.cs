using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Extensions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Common.Projections;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Comments.Queries.ReportComment.GetReportComments;

public sealed class GetReportCommentsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReportCommentsQuery, Result<PagedResult<GetReportCommentsQueryResponse>>>
{
    public async Task<Result<PagedResult<GetReportCommentsQueryResponse>>> Handle(GetReportCommentsQuery request, CancellationToken cancellationToken)
    {
        if (!await context.Reports.AsNoTracking().AnyAsync(report => report.Id == request.ReportId, cancellationToken))
        {
            throw new NotFoundException(nameof(Report), request.ReportId);
        }

        var comments = await context.ReportComments
            .AsNoTracking()
            .Where(comment => comment.ReportId == request.ReportId)
            .OrderBy(comment => comment.CreatedAt)
            .ThenBy(comment => comment.Id)
            .Select(comment => new GetReportCommentsQueryResponse(
                comment.Id,
                comment.UserId,
                comment.User == null ? "Silinmiş Kullanıcı"
                    : UserProjections.FullName.Invoke(comment.User),
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt))
            .Expand()
            .ToPagedResultAsync(request.PageIndex, request.PageSize, cancellationToken);

        return Result<PagedResult<GetReportCommentsQueryResponse>>.Success(comments);
    }
}
