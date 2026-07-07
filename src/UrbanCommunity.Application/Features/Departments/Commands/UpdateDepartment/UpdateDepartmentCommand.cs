using MediatR;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Departments.Commands.UpdateDepartment;

public sealed record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string? Description) : IRequest<Result<UpdateDepartmentCommandResponse>>, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeysToRemove => [CacheKeys.DepartmentsList];
    public IReadOnlyCollection<string> CachePrefixesToRemove => [];
}
