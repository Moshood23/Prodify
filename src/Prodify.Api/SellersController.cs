using MediatR;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Sellers.Commands.ApproveSeller;
using Prodify.Application.Sellers.Commands.RegisterSeller;
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

    [HttpPost]
    public async Task<IActionResult> Register(RegisterSellerCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSellerQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveSellerCommand { SellerId = id }, cancellationToken);
        return NoContent();
    }
}