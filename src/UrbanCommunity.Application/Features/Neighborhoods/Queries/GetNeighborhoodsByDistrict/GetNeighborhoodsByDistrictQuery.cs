using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodsByDistrict;

public sealed record GetNeighborhoodsByDistrictQuery(Guid DistrictId)
    : IRequest<Result<IReadOnlyList<GetNeighborhoodsByDistrictQueryResponse>>>;
