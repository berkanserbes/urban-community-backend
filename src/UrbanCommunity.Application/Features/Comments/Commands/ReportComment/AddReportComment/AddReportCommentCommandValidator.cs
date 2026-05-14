using FluentValidation;

namespace UrbanCommunity.Application.Features.Comments.Commands.ReportComment.AddReportComment;

public sealed class AddReportCommentCommandValidator : AbstractValidator<AddReportCommentCommand>
{
    public AddReportCommentCommandValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty().WithMessage("Rapor kimliği gereklidir.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirlenemedi.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Yorum içeriği boş olamaz.")
            .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir.");
    }
}
