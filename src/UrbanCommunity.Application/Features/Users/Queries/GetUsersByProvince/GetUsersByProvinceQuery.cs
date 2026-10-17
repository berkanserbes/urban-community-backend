using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByProvince;

public sealed record GetUsersByProvinceQuery(Guid ProvinceId)
    : PaginationRequest, IRequest<Result<PagedResult<GetUsersByProvinceQueryResponse>>>;
