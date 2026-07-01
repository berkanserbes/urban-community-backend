using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.CreateOrganizationDepartment;

public sealed record CreateOrganizationDepartmentCommand(
    Guid OrganizationId,
    Guid DepartmentId,
    string? DisplayName,
    string? ContactEmail,
    string? ContactPhone)
    : IRequest<Result<CreateOrganizationDepartmentCommandResponse>>;
