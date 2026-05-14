using FluentValidation;

namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed class MarkAllNotificationsAsReadCommandValidator : AbstractValidator<MarkAllNotificationsAsReadCommand>
{
    public MarkAllNotificationsAsReadCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
