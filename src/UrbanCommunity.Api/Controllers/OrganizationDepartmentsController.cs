using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.OrganizationDepartments.Commands.DeleteOrganizationDepartment;
using UrbanCommunity.Application.Features.OrganizationDepartments.Commands.SetOrganizationDepartmentActive;
using UrbanCommunity.Application.Features.OrganizationDepartments.Commands.SetOrganizationDepartmentCategories;
using UrbanCommunity.Application.Features.OrganizationDepartments.Commands.UpdateOrganizationDepartment;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/organization-departments")]
[Authorize(Roles = "Admin")]
public sealed class OrganizationDepartmentsController(IMediator mediator) : ApiController
{
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganizationDepartmentRequest request)
        => Ok(await mediator.Send(new UpdateOrganizationDepartmentCommand(
            id,
            request.DisplayName,
            request.ContactEmail,
            request.ContactPhone)));

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
        => Ok(await mediator.Send(new SetOrganizationDepartmentActiveCommand(id, true)));

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
        => Ok(await mediator.Send(new SetOrganizationDepartmentActiveCommand(id, false)));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => Ok(await mediator.Send(new DeleteOrganizationDepartmentCommand(id)));

    [HttpPut("{id:guid}/categories")]
    public async Task<IActionResult> SetCategories(Guid id, [FromBody] SetCategoriesRequest request)
        => Ok(await mediator.Send(new SetOrganizationDepartmentCategoriesCommand(id, request.CategoryIds)));
}

public sealed record UpdateOrganizationDepartmentRequest(
    string? DisplayName,
    string? ContactEmail,
    string? ContactPhone);

public sealed record SetCategoriesRequest(IReadOnlyList<Guid> CategoryIds);
