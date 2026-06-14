using FluentValidation;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.ChangeReportStatus;

public sealed class ChangeReportStatusCommandValidator : AbstractValidator<ChangeReportStatusCommand>
{
    public ChangeReportStatusCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("Rapor kimliği gereklidir.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Geçerli bir rapor durumu seçilmelidir.");

        RuleFor(x => x.NewStatus)
            .NotEqual(ReportStatus.AiReview)
            .WithMessage("AI inceleme durumu yalnızca sistem tarafından başlatılabilir.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("İşlemi yapan kullanıcı kimliği gereklidir.");

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Not en fazla 500 karakter olabilir.")
            .When(x => x.Note is not null);

        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("Rapor reddedilirken açıklama girilmelidir.")
            .When(x => x.NewStatus == ReportStatus.Rejected);
    }
}
