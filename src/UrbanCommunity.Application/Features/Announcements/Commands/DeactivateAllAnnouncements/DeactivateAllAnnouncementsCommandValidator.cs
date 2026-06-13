using FluentValidation;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeactivateAllAnnouncements;

public sealed class DeactivateAllAnnouncementsCommandValidator
    : AbstractValidator<DeactivateAllAnnouncementsCommand>
{
    public DeactivateAllAnnouncementsCommandValidator()
    {
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
