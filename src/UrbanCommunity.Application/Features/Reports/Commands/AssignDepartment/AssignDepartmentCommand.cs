using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Reports.Commands.AssignDepartment;

public sealed record AssignDepartmentCommand(
    Guid ReportId,
    Guid? OrganizationDepartmentId,
    Guid ChangedByUserId) : IRequest<Result<AssignDepartmentCommandResponse>>;
