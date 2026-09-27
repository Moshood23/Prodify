using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Admin.Customers.Queries.GetCustomerDetails;
using Prodify.Application.Admin.Customers.Queries.ListCustomers;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

// Customers, for admins. A customer's orders: GET /api/admin/orders?customerId=...
[ApiController]
[Route("api/admin/customers")]
[Authorize(Roles = Roles.Admin)]
public class AdminCustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListCustomersQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerDetailsQuery { Id = id }, cancellationToken);
        return Ok(result);
    }
}