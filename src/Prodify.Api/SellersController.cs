using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Sellers.Commands.ApproveSeller;
using Prodify.Application.Sellers.Commands.RegisterSeller;
using Prodify.Application.Sellers.Queries.GetSeller;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SellersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public SellersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // Any logged-in user can apply to become a seller. The response contains a new token
    // with the Seller role; the account stays "PendingVerification" until an admin approves it.
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterSellerCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.SellerId }, result);
    }

    [Authorize(Roles = Roles.Seller)]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSellerQuery { Id = _currentUser.GetRequiredSellerId() }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSellerQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveSellerCommand { SellerId = id }, cancellationToken);
        return NoContent();
    }
}