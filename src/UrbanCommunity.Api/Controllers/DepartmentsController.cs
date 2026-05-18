using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.Departments.Commands.CreateDepartment;
using UrbanCommunity.Application.Features.Departments.Commands.DeleteAllDepartments;
using UrbanCommunity.Application.Features.Departments.Commands.DeleteDepartment;
using UrbanCommunity.Application.Features.Departments.Commands.UpdateDepartment;
using UrbanCommunity.Application.Features.Departments.Queries.GetDepartmentById;
using UrbanCommunity.Application.Features.Departments.Queries.GetDepartments;
using UrbanCommunity.Application.Features.Departments.Queries.GetDepartmentWithStats;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ApiController
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetDepartmentsQuery());
        return Ok(result);
    }

    [HttpGet("with-stats")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> GetWithStats()
    {
        var result = await _mediator.Send(new GetDepartmentWithStatsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDepartmentByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentCommand command)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match the request body id.");

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteDepartmentCommand(id));
        return Ok(result);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAll()
    {
        var result = await _mediator.Send(new DeleteAllDepartmentsCommand());
        return Ok(result);
    }
}
