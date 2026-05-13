using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Departments.Commands.UpdateDepartment;

public sealed record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone) : IRequest<Result<UpdateDepartmentCommandResponse>>;
