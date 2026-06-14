using FluentValidation;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReportById;

public sealed class GetReportByIdQueryValidator : AbstractValidator<GetReportByIdQuery>
{
    public GetReportByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Rapor kimliği gereklidir.");
    }
}
