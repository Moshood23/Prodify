using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Admin.Payouts.Commands.ProcessPayout;
using Prodify.Application.Admin.Payouts.Queries.ListAdminPayouts;
using Prodify.Application.Admin.Settings.Commands.UpdatePlatformSettings;
using Prodify.Application.Admin.Settings.Queries.GetPlatformSettings;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

// Seller payout requests and the commission rate, for admins.
[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminPayoutsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminPayoutsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("payouts")]
    public async Task<IActionResult> List([FromQuery] ListAdminPayoutsQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    public class MarkPaidRequest
    {
        public string Reference { get; set; } = null!;
    }

    [HttpPost("payouts/{id:guid}/pay")]
    public async Task<IActionResult> MarkPaid(Guid id, MarkPaidRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ProcessPayoutCommand { PayoutId = id, Paid = true, Reference = request.Reference }, cancellationToken);
        return NoContent();
    }

    public class RejectRequest
    {
        public string Reason { get; set; } = null!;
    }

    [HttpPost("payouts/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, RejectRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ProcessPayoutCommand { PayoutId = id, Paid = false, Reason = request.Reason }, cancellationToken);
        return NoContent();
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPlatformSettingsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings(UpdatePlatformSettingsCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
