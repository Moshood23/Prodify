using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Inventory.Commands.CreateWarehouse;
using Prodify.Application.Inventory.Commands.ReserveStock;
using Prodify.Application.Inventory.Queries.GetInventory;
using Prodify.Application.Inventory.Commands.CreateInventoryItem;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.SellerOrAdmin)]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Warehouses are Prodify fulfilment centres, so only admins create them.
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("warehouses")]
    public async Task<IActionResult> CreateWarehouse(CreateWarehouseCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetInventoryQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // Stock is reserved by checkout; calling this directly is an admin-only tool.
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve(ReserveStockCommand command, CancellationToken cancellationToken)
    {
        var reservationId = await _mediator.Send(command, cancellationToken);
        return Ok(reservationId);
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem(CreateInventoryItemCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return Ok(id);
    }
}