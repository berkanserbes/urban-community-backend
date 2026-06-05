using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Provinces.Queries.GetProvinceById;

public sealed record GetProvinceByIdQuery(Guid ProvinceId)
    : IRequest<Result<GetProvinceByIdQueryResponse>>;
