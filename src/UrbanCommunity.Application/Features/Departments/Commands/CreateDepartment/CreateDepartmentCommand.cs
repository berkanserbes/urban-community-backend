using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Departments.Commands.CreateDepartment;

public sealed record CreateDepartmentCommand(
    string Name,
    string? Description,
    string? ContactEmail,
    string? ContactPhone) : IRequest<Result<CreateDepartmentCommandResponse>>;
