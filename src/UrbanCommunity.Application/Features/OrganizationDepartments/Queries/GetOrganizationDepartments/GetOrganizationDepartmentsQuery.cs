using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Queries.GetOrganizationDepartments;

public sealed record GetOrganizationDepartmentsQuery(
    Guid OrganizationId,
    bool? IsActive = null)
    : IRequest<Result<IReadOnlyList<GetOrganizationDepartmentsQueryResponse>>>;
