using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Security;
using Prodify.Application.Sellers.Commands.ReapplySeller;
using Prodify.Application.Sellers.Commands.RegisterSeller;
using Prodify.Application.Sellers.Queries.GetMySeller;
using Prodify.Application.Sellers.Queries.GetSeller;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SellersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SellersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Any logged-in user can apply to become a seller. The response contains a new token
    // with the Seller role; the store stays "PendingVerification" until an admin approves it.
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
        var result = await _mediator.Send(new GetMySellerQuery(), cancellationToken);
        return Ok(result);
    }

    // A rejected seller fixes their details and applies again.
    [Authorize(Roles = Roles.Seller)]
    [HttpPost("me/reapply")]
    public async Task<IActionResult> Reapply(ReapplySellerCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // Public store page information (approved sellers only).
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSellerQuery { Id = id }, cancellationToken);
        return Ok(result);
    }
}