using FluentValidation;

namespace UrbanCommunity.Application.Features.Districts.Queries.GetDistrictById;

public sealed class GetDistrictByIdQueryValidator : AbstractValidator<GetDistrictByIdQuery>
{
    public GetDistrictByIdQueryValidator()
    {
        RuleFor(x => x.DistrictId).NotEmpty();
    }
}
