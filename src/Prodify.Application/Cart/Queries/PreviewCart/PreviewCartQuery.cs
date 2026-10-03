using MediatR;
using Prodify.Application.Cart.Common;
using Prodify.Application.Cart.Queries.GetCart;

namespace Prodify.Application.Cart.Queries.PreviewCart;

// Prices a guest's cart (kept in their browser) at today's prices and stock.
// Each item's Id is its variant id, since nothing is saved.
public class PreviewCartQuery : IRequest<CartDto>
{
    public List<GuestCartItem> Items { get; set; } = new();
}
