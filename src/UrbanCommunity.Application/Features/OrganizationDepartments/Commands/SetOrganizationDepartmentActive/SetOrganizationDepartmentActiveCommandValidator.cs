using FluentValidation;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.SetOrganizationDepartmentActive;

public sealed class SetOrganizationDepartmentActiveCommandValidator
    : AbstractValidator<SetOrganizationDepartmentActiveCommand>
{
    public SetOrganizationDepartmentActiveCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
