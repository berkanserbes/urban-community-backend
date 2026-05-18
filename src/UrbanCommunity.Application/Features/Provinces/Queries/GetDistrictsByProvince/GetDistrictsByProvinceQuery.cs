using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Provinces.Queries.GetDistrictsByProvince;

public sealed record GetDistrictsByProvinceQuery(Guid ProvinceId)
    : IRequest<Result<IReadOnlyList<GetDistrictsByProvinceQueryResponse>>>;
