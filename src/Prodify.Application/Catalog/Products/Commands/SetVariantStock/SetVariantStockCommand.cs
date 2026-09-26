using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.SetVariantStock;

// "I have N units ready to sell." Units already reserved by open orders are kept aside.
public class SetVariantStockCommand : IRequest
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }
}