using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Security;
using Prodify.Application.Returns.Commands;
using Prodify.Application.Returns.Queries;

namespace Prodify.Api.Controllers;

// Returns, for the three people involved: the customer asks, the seller answers, Prodify has the last word.

[ApiController]
[Route("api/returns")]
[Authorize(Roles = Roles.Customer)]
public class ReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReturnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(RequestReturnCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    // After the seller says no: ask Prodify to review it.
    [HttpPost("{id:guid}/escalate")]
    public async Task<IActionResult> Escalate(Guid id, [FromBody] ReturnNote body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new EscalateReturnCommand { ReturnId = id, Note = body.Note }, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/seller-returns")]
[Authorize(Roles = Roles.Seller)]
public class SellerReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SellerReturnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // status: "open", a status name (Requested, Rejected, Escalated, Refunded, Declined), or empty for all.
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListSellerReturnsQuery { Status = status }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AcceptReturnCommand { ReturnId = id }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReturnNote body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RejectReturnCommand { ReturnId = id, Note = body.Note ?? "" }, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/admin/returns")]
[Authorize(Roles = Roles.Admin)]
public class AdminReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminReturnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListAdminReturnsQuery { Status = status }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ReturnNote body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveReturnCommand { ReturnId = id, Note = body.Note }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/decline")]
    public async Task<IActionResult> Decline(Guid id, [FromBody] ReturnNote body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeclineReturnCommand { ReturnId = id, Note = body.Note ?? "" }, cancellationToken);
        return NoContent();
    }
}

public record ReturnNote(string? Note);
