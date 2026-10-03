using MediatR;
using Prodify.Application.Cart.Common;

namespace Prodify.Application.Cart.Commands.MergeGuestCart;

// After a guest logs in or signs up, moves what they put in their browser cart
// into their account cart. Returns notes about anything that couldn't be moved in full.
public class MergeGuestCartCommand : IRequest<List<string>>
{
    public List<GuestCartItem> Items { get; set; } = new();
}
