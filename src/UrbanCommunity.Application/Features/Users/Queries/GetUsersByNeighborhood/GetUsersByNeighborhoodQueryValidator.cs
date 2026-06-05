using FluentValidation;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByNeighborhood;

public sealed class GetUsersByNeighborhoodQueryValidator : AbstractValidator<GetUsersByNeighborhoodQuery>
{
    public GetUsersByNeighborhoodQueryValidator()
    {
        RuleFor(x => x.NeighborhoodId).NotEmpty();
    }
}
