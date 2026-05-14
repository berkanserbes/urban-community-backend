using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Comments.Queries.ReportComment.GetReportCommentById;

public sealed class GetReportCommentByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReportCommentByIdQuery, Result<GetReportCommentByIdQueryResponse>>
{
    public async Task<Result<GetReportCommentByIdQueryResponse>> Handle(GetReportCommentByIdQuery request, CancellationToken cancellationToken)
    {
        var comment = await context.ReportComments
            .AsNoTracking()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(ReportComment), request.CommentId);

        return Result<GetReportCommentByIdQueryResponse>.Success(
            new GetReportCommentByIdQueryResponse(
                comment.Id,
                comment.UserId,
                comment.User.FullName,
                comment.Content,
                comment.CreatedAt,
                comment.UpdatedAt));
    }
}
