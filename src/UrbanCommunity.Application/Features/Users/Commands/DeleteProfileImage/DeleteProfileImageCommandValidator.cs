using FluentValidation;

namespace UrbanCommunity.Application.Features.Users.Commands.DeleteProfileImage;

public sealed class DeleteProfileImageCommandValidator : AbstractValidator<DeleteProfileImageCommand>
{
    public DeleteProfileImageCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
