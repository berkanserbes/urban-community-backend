using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using ReportCommentEntity = UrbanCommunity.Domain.Entities.ReportComment;

namespace UrbanCommunity.Application.Features.Comments.Commands.ReportComment.AddReportComment;

public sealed class AddReportCommentCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddReportCommentCommand, Result<AddReportCommentCommandResponse>>
{
    public async Task<Result<AddReportCommentCommandResponse>> Handle(AddReportCommentCommand request, CancellationToken cancellationToken)
    {
        if (!await context.Reports.AsNoTracking().AnyAsync(report => report.Id == request.ReportId, cancellationToken))
        {
            throw new NotFoundException(nameof(Report), request.ReportId);
        }

        if (!await context.Users.AsNoTracking().AnyAsync(user => user.Id == request.UserId && user.IsActive, cancellationToken))
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        var comment = new ReportCommentEntity
        {
            ReportId = request.ReportId,
            UserId = request.UserId,
            Content = request.Content.Trim()
        };

        context.ReportComments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        return Result<AddReportCommentCommandResponse>.Success(new AddReportCommentCommandResponse(
            comment.Id,
            comment.UserId,
            comment.Content,
            comment.CreatedAt));
    }
}
