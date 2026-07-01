using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.OrganizationDepartments.Commands.CreateOrganizationDepartment;
using UrbanCommunity.Application.Features.OrganizationDepartments.Queries.GetOrganizationDepartments;
using UrbanCommunity.Application.Features.Organizations.Commands.CreateOrganization;
using UrbanCommunity.Application.Features.Organizations.Commands.DeleteOrganization;
using UrbanCommunity.Application.Features.Organizations.Commands.SetOrganizationActive;
using UrbanCommunity.Application.Features.Organizations.Commands.UpdateOrganization;
using UrbanCommunity.Application.Features.Organizations.Queries.GetOrganizationById;
using UrbanCommunity.Application.Features.Organizations.Queries.GetOrganizations;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize(Roles = "Admin")]
public sealed class OrganizationsController(IMediator mediator) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
        => Ok(await mediator.Send(new GetOrganizationsQuery(isActive)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await mediator.Send(new GetOrganizationByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationCommand command)
        => Ok(await mediator.Send(command));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganizationRequest request)
        => Ok(await mediator.Send(new UpdateOrganizationCommand(
            id,
            request.Name,
            request.JurisdictionLevel,
            request.ProvinceId,
            request.DistrictId,
            request.NeighborhoodId)));

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
        => Ok(await mediator.Send(new SetOrganizationActiveCommand(id, true)));

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
        => Ok(await mediator.Send(new SetOrganizationActiveCommand(id, false)));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => Ok(await mediator.Send(new DeleteOrganizationCommand(id)));

    [HttpGet("{organizationId:guid}/departments")]
    public async Task<IActionResult> GetDepartments(Guid organizationId, [FromQuery] bool? isActive)
        => Ok(await mediator.Send(new GetOrganizationDepartmentsQuery(organizationId, isActive)));

    [HttpPost("{organizationId:guid}/departments")]
    public async Task<IActionResult> CreateDepartment(
        Guid organizationId,
        [FromBody] CreateOrganizationDepartmentRequest request)
        => Ok(await mediator.Send(new CreateOrganizationDepartmentCommand(
            organizationId,
            request.DepartmentId,
            request.DisplayName,
            request.ContactEmail,
            request.ContactPhone)));
}

public sealed record UpdateOrganizationRequest(
    string Name,
    UrbanCommunity.Domain.Enums.OrganizationJurisdictionLevel JurisdictionLevel,
    Guid ProvinceId,
    Guid? DistrictId,
    Guid? NeighborhoodId);

public sealed record CreateOrganizationDepartmentRequest(
    Guid DepartmentId,
    string? DisplayName,
    string? ContactEmail,
    string? ContactPhone);
