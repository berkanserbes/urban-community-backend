using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Districts.Queries.GetDistrictById;

public sealed record GetDistrictByIdQuery(Guid DistrictId)
    : IRequest<Result<GetDistrictByIdQueryResponse>>;
