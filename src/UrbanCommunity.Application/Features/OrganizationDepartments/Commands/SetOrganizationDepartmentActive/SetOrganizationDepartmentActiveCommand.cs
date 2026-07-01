using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.SetOrganizationDepartmentActive;

public sealed record SetOrganizationDepartmentActiveCommand(
    Guid Id,
    bool IsActive)
    : IRequest<Result<SetOrganizationDepartmentActiveCommandResponse>>;
