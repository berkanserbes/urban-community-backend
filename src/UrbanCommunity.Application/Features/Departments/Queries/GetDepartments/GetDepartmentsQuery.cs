using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Departments.Queries.GetDepartments;

public sealed record GetDepartmentsQuery : IRequest<Result<IReadOnlyList<GetDepartmentsQueryResponse>>>;
