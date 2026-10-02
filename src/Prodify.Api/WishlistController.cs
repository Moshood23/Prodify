using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Security;
using Prodify.Application.Wishlist.Commands.RemoveFromWishlist;
using Prodify.Application.Wishlist.Commands.SaveToWishlist;
using Prodify.Application.Wishlist.Queries.GetWishlist;
using Prodify.Application.Wishlist.Queries.GetWishlistIds;

namespace Prodify.Api.Controllers;

// The customer's saved products.
[ApiController]
[Route("api/wishlist")]
[Authorize(Roles = Roles.Customer)]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;

    public WishlistController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetWishlistQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("ids")]
    public async Task<IActionResult> Ids(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWishlistIdsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{productId:guid}")]
    public async Task<IActionResult> Save(Guid productId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SaveToWishlistCommand { ProductId = productId }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> Remove(Guid productId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveFromWishlistCommand { ProductId = productId }, cancellationToken);
        return NoContent();
    }
}
