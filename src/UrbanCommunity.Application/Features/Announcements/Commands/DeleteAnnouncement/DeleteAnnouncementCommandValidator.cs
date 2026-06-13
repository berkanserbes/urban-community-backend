using FluentValidation;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeleteAnnouncement;

public sealed class DeleteAnnouncementCommandValidator : AbstractValidator<DeleteAnnouncementCommand>
{
    public DeleteAnnouncementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
