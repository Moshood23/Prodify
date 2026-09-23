using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Shipping.Commands.CreateShipment;
using Prodify.Application.Shipping.Commands.MarkDelivered;
using Prodify.Application.Shipping.Commands.ShipOrder;
using Prodify.Application.Shipping.Commands.UpdateShipmentStatus;
using Prodify.Application.Shipping.Queries.GetShipment;
using Prodify.Application.Shipping.Queries.GetTracking;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShipmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShipmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetShipmentQuery { ShipmentId = id }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/tracking")]
    public async Task<IActionResult> GetTracking(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTrackingQuery { ShipmentId = id }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> Ship(Guid id, ShipOrderCommand command, CancellationToken cancellationToken)
    {
        command.ShipmentId = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateShipmentStatusCommand command, CancellationToken cancellationToken)
    {
        command.ShipmentId = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = Roles.SellerOrAdmin)]
    [HttpPost("{id:guid}/deliver")]
    public async Task<IActionResult> MarkDelivered(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkDeliveredCommand { ShipmentId = id }, cancellationToken);
        return NoContent();
    }
}