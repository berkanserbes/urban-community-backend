using FluentValidation;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;

public sealed class GetAnnouncementsQueryValidator : AbstractValidator<GetAnnouncementsQuery>
{
    public GetAnnouncementsQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => CountLocationFilters(x) <= 1)
            .WithMessage("Aynı anda yalnızca provinceId, districtId veya neighborhoodId filtrelerinden biri gönderilebilir.");
    }

    private static int CountLocationFilters(GetAnnouncementsQuery query)
    {
        var count = 0;

        if (query.ProvinceId.HasValue)
            count++;

        if (query.DistrictId.HasValue)
            count++;

        if (query.NeighborhoodId.HasValue)
            count++;

        return count;
    }
}
