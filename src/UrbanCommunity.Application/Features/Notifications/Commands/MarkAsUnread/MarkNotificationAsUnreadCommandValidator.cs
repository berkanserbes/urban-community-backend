using FluentValidation;

namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAsUnread;

public sealed class MarkNotificationAsUnreadCommandValidator : AbstractValidator<MarkNotificationAsUnreadCommand>
{
    public MarkNotificationAsUnreadCommandValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
