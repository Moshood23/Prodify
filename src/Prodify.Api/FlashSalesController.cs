using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Security;
using Prodify.Application.Promotions.FlashSales.Commands;
using Prodify.Application.Promotions.FlashSales.Queries;

namespace Prodify.Api.Controllers;

// Flash sales: shoppers browse them, sellers set them up, Prodify approves them.

[ApiController]
[Route("api/deals")]
[AllowAnonymous]
public class DealsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DealsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // The flash sales running now, and the ones starting in the next few days.
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDealsQuery(), cancellationToken);
        return Ok(result);
    }
}

[ApiController]
[Route("api/seller-flash-sales")]
[Authorize(Roles = Roles.Seller)]
public class SellerFlashSalesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SellerFlashSalesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // state: pending, live, upcoming, ended, or empty for all.
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? state, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListSellerFlashSalesQuery { State = state }, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFlashSaleCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelFlashSaleCommand { FlashSaleId = id }, cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/admin/flash-sales")]
[Authorize(Roles = Roles.Admin)]
public class AdminFlashSalesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminFlashSalesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? state, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListAdminFlashSalesQuery { State = state }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveFlashSaleCommand { FlashSaleId = id }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] FlashSaleNote body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RejectFlashSaleCommand { FlashSaleId = id, Note = body.Note ?? "" }, cancellationToken);
        return NoContent();
    }

    // End an approved sale early.
    [HttpPost("{id:guid}/stop")]
    public async Task<IActionResult> Stop(Guid id, [FromBody] FlashSaleNote body, CancellationToken cancellationToken)
    {
        await _mediator.Send(new StopFlashSaleCommand { FlashSaleId = id, Note = body.Note ?? "" }, cancellationToken);
        return NoContent();
    }
}

public record FlashSaleNote(string? Note);
