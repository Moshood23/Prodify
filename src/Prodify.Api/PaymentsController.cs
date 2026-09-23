using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Payments.Commands.ProcessPayment;
using Prodify.Application.Payments.Commands.RetryPayment;
using Prodify.Application.Payments.Queries.GetPayment;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = Roles.Customer)]
    [HttpPost]
    public async Task<IActionResult> Process(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var paymentId = await _mediator.Send(command, cancellationToken);
        return Ok(paymentId);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetPaymentQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id, [FromBody] string paymentMethodToken, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RetryPaymentCommand { PaymentId = id, PaymentMethodToken = paymentMethodToken }, cancellationToken);
        return NoContent();
    }
}