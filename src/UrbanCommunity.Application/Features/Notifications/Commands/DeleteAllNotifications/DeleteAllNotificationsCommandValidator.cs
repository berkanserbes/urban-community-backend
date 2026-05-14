using FluentValidation;

namespace UrbanCommunity.Application.Features.Notifications.Commands.DeleteAllNotifications;

public sealed class DeleteAllNotificationsCommandValidator : AbstractValidator<DeleteAllNotificationsCommand>
{
    public DeleteAllNotificationsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
