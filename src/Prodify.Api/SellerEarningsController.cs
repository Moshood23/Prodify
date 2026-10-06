using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Security;
using Prodify.Application.Payouts.Commands.RequestPayout;
using Prodify.Application.Payouts.Commands.SavePayoutAccount;
using Prodify.Application.Payouts.Queries.GetMyEarnings;
using Prodify.Application.Reports.Queries.GetSalesChart;

namespace Prodify.Api.Controllers;

// A seller's earnings, bank account and payout requests.
[ApiController]
[Route("api/seller/earnings")]
[Authorize(Roles = Roles.Seller)]
public class SellerEarningsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SellerEarningsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyEarningsQuery(), cancellationToken);
        return Ok(result);
    }
    [HttpGet("sales")]
    public async Task<IActionResult> Sales([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSalesChartQuery { Days = days, OwnSalesOnly = true }, cancellationToken);
        return Ok(result);
    }


    [HttpPut("account")]
    public async Task<IActionResult> SaveAccount(SavePayoutAccountCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("payouts")]
    public async Task<IActionResult> RequestPayout(RequestPayoutCommand command, CancellationToken cancellationToken)
    {
        var payoutId = await _mediator.Send(command, cancellationToken);
        return Ok(payoutId);
    }
}
