using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Security;
using Prodify.Application.Ordering.SellerOrders.Commands.ChangeSellerOrderStatus;
using Prodify.Application.Ordering.SellerOrders.Queries.GetSellerOrder;
using Prodify.Application.Ordering.SellerOrders.Queries.GetSellerOrderCounts;
using Prodify.Application.Ordering.SellerOrders.Queries.ListSellerOrders;

namespace Prodify.Api.Controllers;

// Seller Centre orders: a seller's part of each customer order.
// Sellers see their own; admins pass ?sellerId= on the lists.
[ApiController]
[Route("api/seller-orders")]
[Authorize(Roles = Roles.SellerOrAdmin)]
public class SellerOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SellerOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListSellerOrdersQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("counts")]
    public async Task<IActionResult> Counts([FromQuery] GetSellerOrderCountsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSellerOrderQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    // {"status": "Confirmed" | "Packed" | "Shipped" | "Delivered" | "Cancelled", "carrier", "trackingNumber", "reason"}
    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, ChangeSellerOrderStatusCommand command, CancellationToken cancellationToken)
    {
        command.SellerOrderId = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}