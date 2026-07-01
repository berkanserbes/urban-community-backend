using FluentValidation;

namespace UrbanCommunity.Application.Features.Organizations.Queries.GetOrganizationById;

public sealed class GetOrganizationByIdQueryValidator
    : AbstractValidator<GetOrganizationByIdQuery>
{
    public GetOrganizationByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty();
    }
}
