using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Payments.Commands.ConfirmPaystackPayment;
using Prodify.Application.Payments.Commands.ProcessPayment;
using Prodify.Application.Payments.Commands.RetryPayment;
using Prodify.Application.Payments.Commands.StartPaystackPayment;
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

    // Tells the website whether to show the test card form or the Paystack button.
    [AllowAnonymous]
    [HttpGet("options")]
    public IActionResult Options([FromServices] IPaymentService paymentService) =>
        Ok(new { provider = paymentService.Provider, testMode = paymentService.IsTestMode });

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

    [Authorize(Roles = Roles.Customer)]
    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id, [FromBody] string paymentMethodToken, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RetryPaymentCommand { PaymentId = id, PaymentMethodToken = paymentMethodToken }, cancellationToken);
        return NoContent();
    }

    // Returns the Paystack page to send the customer to.
    [Authorize(Roles = Roles.Customer)]
    [HttpPost("paystack/start")]
    public async Task<IActionResult> StartPaystack(StartPaystackPaymentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    // The customer is back from Paystack: check with Paystack how it went.
    [Authorize(Roles = Roles.Customer)]
    [HttpPost("paystack/verify/{reference}")]
    public async Task<IActionResult> VerifyPaystack(string reference, [FromServices] ICurrentUserService currentUser, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ConfirmPaystackPaymentCommand { Reference = reference, CustomerId = currentUser.GetRequiredCustomerId() },
            cancellationToken);
        return Ok(result);
    }

    // Paystack calls this when a payment goes through, even if the customer closed the page.
    // Set it in the Paystack dashboard: Settings > API Keys & Webhooks > Webhook URL.
    [AllowAnonymous]
    [HttpPost("paystack/webhook")]
    public async Task<IActionResult> PaystackWebhook(
        [FromServices] IPaymentService paymentService,
        [FromServices] ILogger<PaymentsController> logger,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        if (!paymentService.IsGenuineWebhook(payload, Request.Headers["x-paystack-signature"]))
            return Unauthorized();

        string? eventName;
        string? reference;
        try
        {
            using var json = JsonDocument.Parse(payload);
            var root = json.RootElement;
            eventName = root.TryGetProperty("event", out var e) ? e.GetString() : null;
            reference = root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object
                && data.TryGetProperty("reference", out var r) ? r.GetString() : null;
        }
        catch (JsonException)
        {
            return BadRequest();
        }

        // Paystack also sends events for transfers, refunds and so on; only paid charges matter here.
        if (eventName != "charge.success" || string.IsNullOrWhiteSpace(reference))
            return Ok();

        try
        {
            await _mediator.Send(new ConfirmPaystackPaymentCommand { Reference = reference }, cancellationToken);
        }
        catch (NotFoundException)
        {
            // Not a Prodify order, e.g. a payment made from the Paystack dashboard.
            logger.LogWarning("Paystack webhook for unknown reference {Reference}", reference);
        }

        // Anything else that went wrong answers with an error, so Paystack sends the webhook again later.
        return Ok();
    }
}
