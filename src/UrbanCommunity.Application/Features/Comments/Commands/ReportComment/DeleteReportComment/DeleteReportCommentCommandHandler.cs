using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using ReportCommentEntity = UrbanCommunity.Domain.Entities.ReportComment;

namespace UrbanCommunity.Application.Features.Comments.Commands.ReportComment.DeleteReportComment;

public sealed class DeleteReportCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteReportCommentCommand, Result<DeleteReportCommentCommandResponse>>
{
    public async Task<Result<DeleteReportCommentCommandResponse>> Handle(DeleteReportCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await context.ReportComments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(ReportCommentEntity), request.CommentId);

        if (!request.IsAdmin && comment.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu yorumu yalnızca sahibi veya admin silebilir.");

        context.ReportComments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);
        return Result<DeleteReportCommentCommandResponse>.Success(
            new DeleteReportCommentCommandResponse("Yorum başarıyla silindi."));
    }
}
