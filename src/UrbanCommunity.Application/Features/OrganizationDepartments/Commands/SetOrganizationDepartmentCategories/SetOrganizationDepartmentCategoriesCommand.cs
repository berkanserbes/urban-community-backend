using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Commands.SetOrganizationDepartmentCategories;

public sealed record SetOrganizationDepartmentCategoriesCommand(
    Guid Id,
    IReadOnlyList<Guid> CategoryIds)
    : IRequest<Result<SetOrganizationDepartmentCategoriesCommandResponse>>;
