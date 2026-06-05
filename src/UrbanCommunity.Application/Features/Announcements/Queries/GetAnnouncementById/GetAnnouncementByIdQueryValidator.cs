using FluentValidation;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncementById;

public sealed class GetAnnouncementByIdQueryValidator : AbstractValidator<GetAnnouncementByIdQuery>
{
    public GetAnnouncementByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Duyuru ID zorunludur.");
    }
}
