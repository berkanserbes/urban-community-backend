using FluentValidation;

namespace UrbanCommunity.Application.Features.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
