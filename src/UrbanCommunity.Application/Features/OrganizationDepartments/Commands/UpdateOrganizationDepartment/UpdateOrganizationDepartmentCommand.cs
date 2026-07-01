using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.UpdateOrganizationDepartment;

public sealed record UpdateOrganizationDepartmentCommand(
    Guid Id,
    string? DisplayName,
    string? ContactEmail,
    string? ContactPhone)
    : IRequest<Result<UpdateOrganizationDepartmentCommandResponse>>;
