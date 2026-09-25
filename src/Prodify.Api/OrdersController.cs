using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Prodify.Application.Common.Security;
using Prodify.Application.Ordering.Commands.CancelOrder;
using Prodify.Application.Ordering.Queries.GetOrder;
using Prodify.Application.Ordering.Queries.ListMyOrders;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // The logged-in customer's own orders, newest first.
    [Authorize(Roles = Roles.Customer)]
    [HttpGet]
    public async Task<IActionResult> ListMine([FromQuery] ListMyOrdersQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    // The body is optional: {"reason": "..."} or nothing at all.
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CancelOrderRequest? request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelOrderCommand { OrderId = id, Reason = request?.Reason }, cancellationToken);
        return NoContent();
    }
}

public record CancelOrderRequest(string? Reason);