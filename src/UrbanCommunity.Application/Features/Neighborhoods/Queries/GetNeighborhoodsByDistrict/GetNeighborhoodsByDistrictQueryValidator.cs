using FluentValidation;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodsByDistrict;

public sealed class GetNeighborhoodsByDistrictQueryValidator : AbstractValidator<GetNeighborhoodsByDistrictQuery>
{
    public GetNeighborhoodsByDistrictQueryValidator()
    {
        RuleFor(x => x.DistrictId).NotEmpty();
    }
}
