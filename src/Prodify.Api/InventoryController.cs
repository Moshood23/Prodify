using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Inventory.Commands.CreateWarehouse;
using Prodify.Application.Inventory.Commands.ReserveStock;
using Prodify.Application.Inventory.Queries.GetInventory;
using Prodify.Application.Inventory.Commands.CreateInventoryItem;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

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