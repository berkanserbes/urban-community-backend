using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Departments.Commands.DeleteAllDepartments;

public sealed record DeleteAllDepartmentsCommand : IRequest<Result<DeleteAllDepartmentsCommandResponse>>;
