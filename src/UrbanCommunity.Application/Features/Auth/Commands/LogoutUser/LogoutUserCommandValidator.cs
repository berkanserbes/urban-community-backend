using FluentValidation;

namespace UrbanCommunity.Application.Features.Auth.Commands.LogoutUser;

public sealed class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token boş bırakılamaz.");
    }
}
