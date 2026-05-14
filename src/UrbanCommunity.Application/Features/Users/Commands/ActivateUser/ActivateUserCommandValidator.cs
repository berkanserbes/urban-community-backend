using FluentValidation;

namespace UrbanCommunity.Application.Features.Users.Commands.ActivateUser;

public sealed class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
