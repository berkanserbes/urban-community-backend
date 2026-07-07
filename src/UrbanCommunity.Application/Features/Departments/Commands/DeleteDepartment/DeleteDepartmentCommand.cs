using MediatR;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Departments.Commands.DeleteDepartment;

public sealed record DeleteDepartmentCommand(Guid Id)
    : IRequest<Result<DeleteDepartmentCommandResponse>>, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeysToRemove => [CacheKeys.DepartmentsList];
    public IReadOnlyCollection<string> CachePrefixesToRemove => [];
}
