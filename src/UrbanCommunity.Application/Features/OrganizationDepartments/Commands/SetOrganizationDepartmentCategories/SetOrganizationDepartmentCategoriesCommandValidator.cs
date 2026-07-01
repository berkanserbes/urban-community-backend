using FluentValidation;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.SetOrganizationDepartmentCategories;

public sealed class SetOrganizationDepartmentCategoriesCommandValidator
    : AbstractValidator<SetOrganizationDepartmentCategoriesCommand>
{
    public SetOrganizationDepartmentCategoriesCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.CategoryIds)
            .NotNull();

        RuleForEach(command => command.CategoryIds)
            .NotEmpty();
    }
}
