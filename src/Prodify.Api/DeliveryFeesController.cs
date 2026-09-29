using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Security;
using Prodify.Application.Shipping.DeliveryFees.Commands.UpdateDeliveryFee;
using Prodify.Application.Shipping.DeliveryFees.Queries.ListDeliveryFees;

namespace Prodify.Api.Controllers;

// Delivery fee per state: anyone can read them (checkout shows the fee), admins change them.
[ApiController]
[Route("api/delivery-fees")]
public class DeliveryFeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveryFeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListDeliveryFeesQuery(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut]
    public async Task<IActionResult> Update(UpdateDeliveryFeeCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
