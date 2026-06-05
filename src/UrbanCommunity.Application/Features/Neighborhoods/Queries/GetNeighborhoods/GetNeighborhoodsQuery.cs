using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoods;

public sealed record GetNeighborhoodsQuery(
    Guid? ProvinceId = null,
    Guid? DistrictId = null) : IRequest<Result<IReadOnlyList<GetNeighborhoodsQueryResponse>>>;
