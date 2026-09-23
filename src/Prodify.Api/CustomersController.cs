using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
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
    private readonly ICurrentUserService _currentUser;

    public CustomersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // Shoppers sign up through POST /api/auth/register; this endpoint is for admins.
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterCustomerCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [Authorize(Roles = Roles.Customer)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerQuery { Id = _currentUser.GetRequiredCustomerId() }, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.Customer)]
    [HttpPost("me/addresses")]
    public async Task<IActionResult> AddAddress(AddCustomerAddressCommand command, CancellationToken cancellationToken)
    {
        var addressId = await _mediator.Send(command, cancellationToken);
        return Ok(addressId);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCustomerQuery { Id = id }, cancellationToken);
        return Ok(result);
    }
}