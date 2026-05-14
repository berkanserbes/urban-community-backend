using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Users.Commands.AssignRole;

public sealed record AssignRoleCommand(Guid Id, UserRole Role, Guid? DepartmentId) : IRequest<Result<AssignRoleCommandResponse>>;
