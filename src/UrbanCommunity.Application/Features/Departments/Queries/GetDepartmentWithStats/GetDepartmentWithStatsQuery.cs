using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Departments.Queries.GetDepartmentWithStats;

public sealed record GetDepartmentWithStatsQuery : IRequest<Result<IReadOnlyList<GetDepartmentWithStatsQueryResponse>>>;
