using FluentValidation;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.AssignRole;

public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .When(x => x.Role == UserRole.Official)
            .WithMessage("Official rolü için DepartmentId zorunludur.");
    }
}
