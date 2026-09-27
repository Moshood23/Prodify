using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Catalog.Brands.Commands.CreateBrand;
using Prodify.Application.Catalog.Brands.Commands.DeleteBrand;
using Prodify.Application.Catalog.Brands.Commands.UpdateBrand;
using Prodify.Application.Catalog.Brands.Queries.ListBrands;
using Prodify.Application.Catalog.Brands.Queries.ListManagedBrands;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BrandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListBrandsQuery(), cancellationToken);
        return Ok(result);
    }

    // Admin: with product counts.
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("manage")]
    public async Task<IActionResult> ListManaged(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListManagedBrandsQuery(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateBrandCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBrandCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBrandCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}