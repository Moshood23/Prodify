using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Catalog.Categories.Commands.CreateCategory;
using Prodify.Application.Catalog.Categories.Commands.DeleteCategory;
using Prodify.Application.Catalog.Categories.Commands.UpdateCategory;
using Prodify.Application.Catalog.Categories.Queries.ListCategories;
using Prodify.Application.Catalog.Categories.Queries.ListManagedCategories;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    // Admin: with product and sub-category counts.
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("manage")]
    public async Task<IActionResult> ListManaged(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListManagedCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCategoryCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}