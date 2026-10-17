using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Api.Constants;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Reports.Commands.AssignDepartment;
using UrbanCommunity.Application.Features.Reports.Commands.ChangeReportPriority;
using UrbanCommunity.Application.Features.Reports.Commands.ChangeReportStatus;
using UrbanCommunity.Application.Features.Reports.Commands.CreateReport;
using UrbanCommunity.Application.Features.Reports.Commands.DeleteReport;
using UrbanCommunity.Application.Features.Reports.Commands.UpdateReport;
using UrbanCommunity.Application.Features.Reports.Queries.GetAllowedReportStatusTransitions;
using UrbanCommunity.Application.Features.Reports.Queries.GetManageableReports;
using UrbanCommunity.Application.Features.Reports.Queries.GetManageableReportStats;
using UrbanCommunity.Application.Features.Reports.Queries.GetReportAssignmentOptions;
using UrbanCommunity.Application.Features.Reports.Queries.GetReportById;
using UrbanCommunity.Application.Features.Reports.Queries.GetReports;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController(IMediator mediator) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<PagedResult<GetReportsQueryResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationRequest pagination,
        [FromQuery] string? scope,
        [FromQuery] ReportStatus? status,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? assignedOrganizationDepartmentId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetReportsQuery(
                RequestingUserId: GetCurrentUserId(),
                IsOfficial: User.IsInRole("Official"),
                Scope: scope,
                Status: status,
                CategoryId: categoryId,
                AssignedOrganizationDepartmentId: assignedOrganizationDepartmentId)
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            },
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine")]
    [ProducesResponseType(typeof(Result<PagedResult<GetReportsQueryResponse>>), 200)]
    public async Task<IActionResult> GetMine(
        [FromQuery] PaginationRequest pagination,
        [FromQuery] ReportStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetReportsQuery(Status: status, UserId: GetCurrentUserId())
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            },
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("manageable")]
    [Authorize(Roles = "Admin,Official")]
    [ProducesResponseType(typeof(Result<PagedResult<GetReportsQueryResponse>>), 200)]
    public async Task<IActionResult> GetManageable(
        [FromQuery] PaginationRequest pagination,
        [FromQuery] Guid? neighborhoodId,
        [FromQuery] ReportStatus? status,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? assignedOrganizationDepartmentId,
        [FromQuery] string? search,
        [FromQuery] ReportPriority? priority,
        [FromQuery] ReportAssignmentFilter? assignment,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetManageableReportsQuery(
                GetCurrentUserId(),
                User.IsInRole("Admin"),
                neighborhoodId,
                status,
                categoryId,
                assignedOrganizationDepartmentId,
                search,
                priority,
                assignment)
            {
                PageIndex = pagination.PageIndex,
                PageSize = pagination.PageSize
            },
            cancellationToken);

        return Ok(result);
    }

    /// <summary>Returns report counts within the caller's manageable scope.</summary>
    [HttpGet("manageable/stats")]
    [Authorize(Roles = "Admin,Official")]
    [ProducesResponseType(typeof(Result<GetManageableReportStatsQueryResponse>), 200)]
    public async Task<IActionResult> GetManageableStats(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetManageableReportStatsQuery(GetCurrentUserId(), User.IsInRole("Admin")),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetReportByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/allowed-status-transitions")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> GetAllowedStatusTransitions(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAllowedReportStatusTransitionsQuery(
                id,
                GetCurrentUserId(),
                User.IsInRole("Admin")),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/assignment-options")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAssignmentOptions(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetReportAssignmentOptionsQuery(id),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [RequestSizeLimit(ApiConstants.MaxReportUploadSize)]
    public async Task<IActionResult> Create([FromForm] CreateReportRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReportCommand(
            request.Title,
            request.Description,
            request.Latitude,
            request.Longitude,
            request.SuggestedCategoryId,
            request.NeighborhoodId,
            GetCurrentUserId(),
            request.Media);

        var result = await mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [RequestSizeLimit(ApiConstants.MaxReportUploadSize)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromForm] UpdateReportRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReportCommand(
            id,
            GetCurrentUserId(),
            request.Title,
            request.Description,
            request.Latitude,
            request.Longitude,
            request.SuggestedCategoryId,
            request.NeighborhoodId,
            request.DeleteMediaIds,
            request.Media);

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteReportCommand(id, GetCurrentUserId(), User.IsInRole("Admin"));
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeReportStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeReportStatusCommand(
            id,
            request.NewStatus,
            GetCurrentUserId(),
            User.IsInRole("Admin"),
            request.Note);
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/assign-department")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignDepartment(Guid id, [FromBody] AssignDepartmentRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AssignDepartmentCommand(id, request.OrganizationDepartmentId, GetCurrentUserId()),
            cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/priority")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> ChangePriority(
        Guid id,
        [FromBody] ChangeReportPriorityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ChangeReportPriorityCommand(
                id,
                request.Priority,
                GetCurrentUserId(),
                User.IsInRole("Admin")),
            cancellationToken);

        return Ok(result);
    }

    public sealed class CreateReportRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public Guid? SuggestedCategoryId { get; init; }
        public Guid NeighborhoodId { get; init; }
        public IReadOnlyList<IFormFile>? Media { get; init; }
    }

    public sealed class UpdateReportRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public Guid? SuggestedCategoryId { get; init; }
        public Guid NeighborhoodId { get; init; }
        public IReadOnlyList<Guid>? DeleteMediaIds { get; init; }
        public IReadOnlyList<IFormFile>? Media { get; init; }
    }

    public sealed record ChangeReportStatusRequest(ReportStatus NewStatus, string? Note);
    public sealed record ChangeReportPriorityRequest(ReportPriority Priority);
    public sealed record AssignDepartmentRequest(Guid? OrganizationDepartmentId);
}
