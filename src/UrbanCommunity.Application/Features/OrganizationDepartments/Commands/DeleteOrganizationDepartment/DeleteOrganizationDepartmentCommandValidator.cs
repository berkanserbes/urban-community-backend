using FluentValidation;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.DeleteOrganizationDepartment;

public sealed class DeleteOrganizationDepartmentCommandValidator
    : AbstractValidator<DeleteOrganizationDepartmentCommand>
{
    public DeleteOrganizationDepartmentCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
