using FluentValidation;

namespace UrbanCommunity.Application.Features.Announcements.Commands.DeleteAllAnnouncements;

public sealed class DeleteAllAnnouncementsCommandValidator
    : AbstractValidator<DeleteAllAnnouncementsCommand>
{
    public DeleteAllAnnouncementsCommandValidator()
    {
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
