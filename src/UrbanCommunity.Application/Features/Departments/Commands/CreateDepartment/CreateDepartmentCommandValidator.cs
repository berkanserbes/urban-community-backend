using FluentValidation;

namespace UrbanCommunity.Application.Features.Departments.Commands.CreateDepartment;

public sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Departman adı boş olamaz.")
            .MaximumLength(150).WithMessage("Departman adı en fazla 150 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

    }
}
