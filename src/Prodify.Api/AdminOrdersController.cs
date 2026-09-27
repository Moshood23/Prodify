using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Admin.Orders.Queries.GetAdminOrder;
using Prodify.Application.Admin.Orders.Queries.ListAdminOrders;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

// Every order on the platform, for admins.
// A seller's part of an order is managed through /api/seller-orders/{id} (admins can use it too).
[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = Roles.Admin)]
public class AdminOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListAdminOrdersQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminOrderQuery { Id = id }, cancellationToken);
        return Ok(result);
    }
}