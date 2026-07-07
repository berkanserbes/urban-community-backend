using MediatR;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Departments.Commands.CreateDepartment;

public sealed record CreateDepartmentCommand(
    string Name,
    string? Description) : IRequest<Result<CreateDepartmentCommandResponse>>, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeysToRemove => [CacheKeys.DepartmentsList];
    public IReadOnlyCollection<string> CachePrefixesToRemove => [];
}
