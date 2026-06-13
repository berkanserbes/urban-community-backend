using FluentValidation;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeactivateAnnouncement;

public sealed class DeactivateAnnouncementCommandValidator : AbstractValidator<DeactivateAnnouncementCommand>
{
    public DeactivateAnnouncementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
