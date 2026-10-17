using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByDistrict;

public sealed record GetUsersByDistrictQuery(Guid DistrictId)
    : PaginationRequest, IRequest<Result<PagedResult<GetUsersByDistrictQueryResponse>>>;
