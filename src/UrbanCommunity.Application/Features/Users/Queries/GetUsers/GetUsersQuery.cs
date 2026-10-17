using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    string? Search = null,
    UserRole? Role = null,
    bool? IsActive = null) : PaginationRequest, IRequest<Result<PagedResult<GetUsersQueryResponse>>>;
