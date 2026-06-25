using FluentValidation;

namespace UrbanCommunity.Application.Features.Departments.Commands.UpdateDepartment;

public sealed class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Departman kimliği gereklidir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Departman adı boş olamaz.")
            .MaximumLength(150).WithMessage("Departman adı en fazla 150 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

    }
}
