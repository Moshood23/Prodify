using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Catalog.Products.Commands.CreateProduct;
using Prodify.Application.Catalog.Products.Commands.UpdateProduct;
using Prodify.Application.Catalog.Products.Queries.GetProduct;
using Prodify.Application.Catalog.Products.Queries.ListProducts;
using Prodify.Application.Catalog.Products.Commands.CreateProductVariant;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListProductsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost("{productId:guid}/variants")]
    public async Task<IActionResult> CreateVariant(Guid productId, CreateProductVariantCommand command, CancellationToken cancellationToken)
    {
        command.ProductId = productId;
        var id = await _mediator.Send(command, cancellationToken);
        return Ok(id);
    }
}