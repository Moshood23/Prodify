using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Ordering.Commands.CancelOrder;
using Prodify.Application.Ordering.Queries.GetOrder;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string? reason, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelOrderCommand { OrderId = id, Reason = reason }, cancellationToken);
        return NoContent();
    }
}