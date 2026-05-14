using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using ReportCommentEntity = UrbanCommunity.Domain.Entities.ReportComment;

namespace UrbanCommunity.Application.Features.Comments.Commands.ReportComment.UpdateReportComment;

public sealed class UpdateReportCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateReportCommentCommand, Result<UpdateReportCommentCommandResponse>>
{
    public async Task<Result<UpdateReportCommentCommandResponse>> Handle(UpdateReportCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await context.ReportComments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken)
            ?? throw new NotFoundException(nameof(ReportComment), request.CommentId);

        if (comment.UserId != request.RequestingUserId)
            throw new ForbiddenAccessException("Bu yorumu yalnızca sahibi düzenleyebilir.");

        comment.Content = request.Content.Trim();

        await context.SaveChangesAsync(cancellationToken);
        return Result<UpdateReportCommentCommandResponse>.Success(
            new UpdateReportCommentCommandResponse(comment.Id, comment.Content, comment.UpdatedAt));
    }
}
