using FluentValidation;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.UpdateOrganizationDepartment;

public sealed class UpdateOrganizationDepartmentCommandValidator
    : AbstractValidator<UpdateOrganizationDepartmentCommand>
{
    public UpdateOrganizationDepartmentCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.DisplayName)
            .MaximumLength(150);

        RuleFor(command => command.ContactEmail)
            .EmailAddress()
            .MaximumLength(100)
            .When(command => !string.IsNullOrWhiteSpace(command.ContactEmail));

        RuleFor(command => command.ContactPhone)
            .MaximumLength(20);
    }
}
