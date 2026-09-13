using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Customers.Commands.AddCustomerAddress;
using Prodify.Application.Customers.Commands.RegisterCustomer;
using Prodify.Application.Customers.Queries.GetCustomer;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterCustomerCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/addresses")]
    public async Task<IActionResult> AddAddress(Guid id, AddCustomerAddressCommand command, CancellationToken cancellationToken)
    {
        command.CustomerId = id;
        var addressId = await _mediator.Send(command, cancellationToken);
        return Ok(addressId);
    }
}