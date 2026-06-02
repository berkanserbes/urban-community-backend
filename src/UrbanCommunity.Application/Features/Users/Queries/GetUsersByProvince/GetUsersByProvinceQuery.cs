using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByProvince;

public sealed record GetUsersByProvinceQuery(Guid ProvinceId)
    : IRequest<Result<IReadOnlyList<GetUsersByProvinceQueryResponse>>>;
