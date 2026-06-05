using FluentValidation;

namespace UrbanCommunity.Application.Features.Provinces.Queries.GetProvinceById;

public sealed class GetProvinceByIdQueryValidator : AbstractValidator<GetProvinceByIdQuery>
{
    public GetProvinceByIdQueryValidator()
    {
        RuleFor(x => x.ProvinceId).NotEmpty();
    }
}
