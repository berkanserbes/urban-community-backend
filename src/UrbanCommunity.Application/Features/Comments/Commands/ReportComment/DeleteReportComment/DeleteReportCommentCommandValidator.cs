using FluentValidation;

namespace UrbanCommunity.Application.Features.Comments.Commands.ReportComment.DeleteReportComment;

public sealed class DeleteReportCommentCommandValidator : AbstractValidator<DeleteReportCommentCommand>
{
    public DeleteReportCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
