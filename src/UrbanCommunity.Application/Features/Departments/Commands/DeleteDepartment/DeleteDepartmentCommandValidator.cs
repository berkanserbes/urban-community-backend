using FluentValidation;

namespace UrbanCommunity.Application.Features.Departments.Commands.DeleteDepartment;

public sealed class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
