using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Cart.Commands.AddToCart;
using Prodify.Application.Cart.Commands.RemoveCartItem;
using Prodify.Application.Cart.Commands.UpdateCartItemQuantity;
using Prodify.Application.Cart.Queries.GetCart;
using Prodify.Application.Common.Security;

namespace Prodify.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Customer)]
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
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCartQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("items/{cartItemId:guid}")]
    public async Task<IActionResult> UpdateItemQuantity(Guid cartItemId, UpdateCartItemQuantityRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateCartItemQuantityCommand { CartItemId = cartItemId, Quantity = request.Quantity }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveCartItemCommand { CartItemId = cartItemId }, cancellationToken);
        return NoContent();
    }
}

public record UpdateCartItemQuantityRequest(int Quantity);