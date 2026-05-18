using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanCommunity.Application.Features.Categories.Commands.CreateCategory;
using UrbanCommunity.Application.Features.Categories.Commands.DeleteCategory;
using UrbanCommunity.Application.Features.Categories.Commands.UpdateCategory;
using UrbanCommunity.Application.Features.Categories.Queries.GetCategories;
using UrbanCommunity.Application.Features.Categories.Queries.GetCategoryById;
using UrbanCommunity.Application.Features.Categories.Queries.GetCategoriesWithReportCount;

namespace UrbanCommunity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetCategoriesQuery());
        return Ok(result);
    }

    [HttpGet("with-report-count")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWithReportCount()
    {
        var result = await _mediator.Send(new GetCategoriesWithReportCountQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Official")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID uyuşmazlığı.");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id));
        return Ok(result);
    }
}
