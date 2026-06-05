using FluentValidation;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodLocation;

public sealed class GetNeighborhoodLocationQueryValidator : AbstractValidator<GetNeighborhoodLocationQuery>
{
    public GetNeighborhoodLocationQueryValidator()
    {
        RuleFor(x => x.NeighborhoodId).NotEmpty();
    }
}
