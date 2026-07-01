using FluentValidation;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Queries.GetOrganizationDepartments;

public sealed class GetOrganizationDepartmentsQueryValidator
    : AbstractValidator<GetOrganizationDepartmentsQuery>
{
    public GetOrganizationDepartmentsQueryValidator()
    {
        RuleFor(query => query.OrganizationId)
            .NotEmpty();
    }
}
