using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Cart.Commands.AddToCart;
using Prodify.Application.Cart.Commands.RemoveCartItem;
using Prodify.Application.Cart.Queries.GetCart;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddToCartCommand command, CancellationToken cancellationToken)
    {
        var cartId = await _mediator.Send(command, cancellationToken);
        return Ok(cartId);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetCartQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{cartId:guid}/items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartId, Guid cartItemId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveCartItemCommand { CartId = cartId, CartItemId = cartItemId }, cancellationToken);
        return NoContent();
    }
}