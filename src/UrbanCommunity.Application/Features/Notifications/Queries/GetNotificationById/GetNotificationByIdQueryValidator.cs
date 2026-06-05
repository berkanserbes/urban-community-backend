using FluentValidation;

namespace UrbanCommunity.Application.Features.Notifications.Queries.GetNotificationById;

public sealed class GetNotificationByIdQueryValidator : AbstractValidator<GetNotificationByIdQuery>
{
    public GetNotificationByIdQueryValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
