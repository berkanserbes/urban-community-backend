using MediatR;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Departments.Queries.GetDepartments;

public sealed record GetDepartmentsQuery
    : IRequest<Result<IReadOnlyList<GetDepartmentsQueryResponse>>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.DepartmentsList;
    public CacheOptions CacheOptions => CacheDurations.ReferenceData;
}
