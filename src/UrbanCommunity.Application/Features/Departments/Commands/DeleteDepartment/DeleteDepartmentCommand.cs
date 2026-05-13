using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Departments.Commands.DeleteDepartment;

public sealed record DeleteDepartmentCommand(Guid Id) : IRequest<Result<DeleteDepartmentCommandResponse>>;
