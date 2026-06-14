using FluentValidation;

namespace UrbanCommunity.Application.Features.Reports.Commands.ChangeReportPriority;

public sealed class ChangeReportPriorityCommandValidator : AbstractValidator<ChangeReportPriorityCommand>
{
    public ChangeReportPriorityCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("Rapor kimliği gereklidir.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Geçerli bir öncelik seçilmelidir.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("İşlemi yapan kullanıcı kimliği gereklidir.");
    }
}
