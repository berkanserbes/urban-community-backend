using FluentValidation;

namespace UrbanCommunity.Application.Features.Provinces.Queries.GetDistrictsByProvince;

public sealed class GetDistrictsByProvinceQueryValidator : AbstractValidator<GetDistrictsByProvinceQuery>
{
    public GetDistrictsByProvinceQueryValidator()
    {
        RuleFor(x => x.ProvinceId).NotEmpty();
    }
}
