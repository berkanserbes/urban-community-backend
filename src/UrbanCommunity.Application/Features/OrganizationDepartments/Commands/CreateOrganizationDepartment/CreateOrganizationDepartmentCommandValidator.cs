using FluentValidation;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.CreateOrganizationDepartment;

public sealed class CreateOrganizationDepartmentCommandValidator
    : AbstractValidator<CreateOrganizationDepartmentCommand>
{
    public CreateOrganizationDepartmentCommandValidator()
    {
        RuleFor(command => command.OrganizationId)
            .NotEmpty();

        RuleFor(command => command.DepartmentId)
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
