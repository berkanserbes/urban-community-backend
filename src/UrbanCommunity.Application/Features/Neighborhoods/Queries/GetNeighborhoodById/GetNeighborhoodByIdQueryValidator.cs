using FluentValidation;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodById;

public sealed class GetNeighborhoodByIdQueryValidator : AbstractValidator<GetNeighborhoodByIdQuery>
{
    public GetNeighborhoodByIdQueryValidator()
    {
        RuleFor(x => x.NeighborhoodId).NotEmpty();
    }
}
