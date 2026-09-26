using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Catalog.Products.Commands.AddProductImage;
using Prodify.Application.Catalog.Products.Commands.CreateProduct;
using Prodify.Application.Catalog.Products.Commands.CreateProductVariant;
using Prodify.Application.Catalog.Products.Commands.RemoveProductImage;
using Prodify.Application.Catalog.Products.Commands.SetProductActive;
using Prodify.Application.Catalog.Products.Commands.SetProductVariantActive;
using Prodify.Application.Catalog.Products.Commands.SetVariantStock;
using Prodify.Application.Catalog.Products.Commands.UpdateProduct;
using Prodify.Application.Catalog.Products.Commands.UpdateProductAttributes;
using Prodify.Application.Catalog.Products.Commands.UpdateProductVariant;
using Prodify.Application.Catalog.Products.Queries.GetManagedProduct;
using Prodify.Application.Catalog.Products.Queries.GetProduct;
using Prodify.Application.Catalog.Products.Queries.ListManagedProducts;
using Prodify.Application.Catalog.Products.Queries.ListProducts;
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

    // ---- Shop ----

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListProductsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    // ---- Seller Centre (sellers manage their own products; admins manage any) ----

    // Sellers get their own products; admins pass ?sellerId=.
    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpGet("manage")]
    public async Task<IActionResult> ListManaged([FromQuery] ListManagedProductsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpGet("{id:guid}/manage")]
    public async Task<IActionResult> GetManaged(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetManagedProductQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, id);
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
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetProductActiveCommand { Id = id, IsActive = true }, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetProductActiveCommand { Id = id, IsActive = false }, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPut("{productId:guid}/attributes")]
    public async Task<IActionResult> UpdateAttributes(Guid productId, UpdateProductAttributesCommand command, CancellationToken cancellationToken)
    {
        command.ProductId = productId;
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

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPut("{productId:guid}/variants/{variantId:guid}")]
    public async Task<IActionResult> UpdateVariant(Guid productId, Guid variantId, UpdateProductVariantCommand command, CancellationToken cancellationToken)
    {
        command.ProductId = productId;
        command.VariantId = variantId;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost("{productId:guid}/variants/{variantId:guid}/activate")]
    public async Task<IActionResult> ActivateVariant(Guid productId, Guid variantId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetProductVariantActiveCommand { ProductId = productId, VariantId = variantId, IsActive = true }, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost("{productId:guid}/variants/{variantId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateVariant(Guid productId, Guid variantId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetProductVariantActiveCommand { ProductId = productId, VariantId = variantId, IsActive = false }, cancellationToken);
        return NoContent();
    }

    // Sets how many units are ready to sell.
    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPut("{productId:guid}/variants/{variantId:guid}/stock")]
    public async Task<IActionResult> SetVariantStock(Guid productId, Guid variantId, SetVariantStockCommand command, CancellationToken cancellationToken)
    {
        command.ProductId = productId;
        command.VariantId = variantId;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost("{productId:guid}/images")]
    public async Task<IActionResult> AddImage(Guid productId, AddProductImageCommand command, CancellationToken cancellationToken)
    {
        command.ProductId = productId;
        var id = await _mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpDelete("{productId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> RemoveImage(Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveProductImageCommand { ProductId = productId, ImageId = imageId }, cancellationToken);
        return NoContent();
    }
}