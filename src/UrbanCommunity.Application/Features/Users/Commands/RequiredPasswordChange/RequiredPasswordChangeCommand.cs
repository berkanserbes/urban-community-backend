using FluentValidation;
using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Commands.RequiredPasswordChange;

public sealed record RequiredPasswordChangeCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword) : IRequest<Result<RequiredPasswordChangeCommandResponse>>;

public sealed record RequiredPasswordChangeCommandResponse(Guid UserId, string Message);

public sealed class RequiredPasswordChangeCommandValidator
    : AbstractValidator<RequiredPasswordChangeCommand>
{
    public RequiredPasswordChangeCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.CurrentPassword).NotEmpty();
        RuleFor(command => command.NewPassword)
            .NotEmpty()
            .MinimumLength(6)
            .NotEqual(command => command.CurrentPassword);
        RuleFor(command => command.ConfirmNewPassword)
            .Equal(command => command.NewPassword);
    }
}
