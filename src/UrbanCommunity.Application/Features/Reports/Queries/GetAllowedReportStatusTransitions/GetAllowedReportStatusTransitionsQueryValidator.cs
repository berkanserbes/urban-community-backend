using FluentValidation;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetAllowedReportStatusTransitions;

public sealed class GetAllowedReportStatusTransitionsQueryValidator
    : AbstractValidator<GetAllowedReportStatusTransitionsQuery>
{
    public GetAllowedReportStatusTransitionsQueryValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEmpty().WithMessage("Rapor kimliği gereklidir.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("İşlemi yapan kullanıcı kimliği gereklidir.");
    }
}
