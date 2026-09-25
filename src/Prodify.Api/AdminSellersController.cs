using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Security;
using Prodify.Application.Sellers.Commands.ApproveSeller;
using Prodify.Application.Sellers.Commands.ReinstateSeller;
using Prodify.Application.Sellers.Commands.RejectSeller;
using Prodify.Application.Sellers.Commands.SuspendSeller;
using Prodify.Application.Sellers.Queries.GetSellerDetails;
using Prodify.Application.Sellers.Queries.ListSellers;

namespace Prodify.Api.Controllers;

// Seller review and management for admins.
[ApiController]
[Route("api/admin/sellers")]
[Authorize(Roles = Roles.Admin)]
public class AdminSellersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminSellersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ListSellersQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSellerDetailsQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveSellerCommand { SellerId = id }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, SellerStatusReasonRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RejectSellerCommand { SellerId = id, Reason = request.Reason }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, SellerStatusReasonRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SuspendSellerCommand { SellerId = id, Reason = request.Reason }, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reinstate")]
    public async Task<IActionResult> Reinstate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReinstateSellerCommand { SellerId = id }, cancellationToken);
        return NoContent();
    }
}

public record SellerStatusReasonRequest(string Reason);