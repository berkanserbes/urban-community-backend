using FluentValidation;

namespace UrbanCommunity.Application.Features.Reports.Commands.DeleteReport;

public sealed class DeleteReportCommandValidator : AbstractValidator<DeleteReportCommand>
{
    public DeleteReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("Rapor kimliği gereklidir.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği gereklidir.");
    }
}
