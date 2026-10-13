using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.SetVariantStock;

// "I have N units ready to sell." Units already reserved by open orders are kept aside.
public class SetVariantStockCommand : IRequest
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public int Quantity { get; set; }

    // Warn me when it drops to this many or fewer (0 = never). Left as it is when not sent.
    public int? LowStockThreshold { get; set; }
}
