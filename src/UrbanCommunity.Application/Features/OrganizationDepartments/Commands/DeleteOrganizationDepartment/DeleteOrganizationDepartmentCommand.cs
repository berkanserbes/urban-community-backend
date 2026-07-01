using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.DeleteOrganizationDepartment;

public sealed record DeleteOrganizationDepartmentCommand(Guid Id)
    : IRequest<Result<DeleteOrganizationDepartmentCommandResponse>>;
