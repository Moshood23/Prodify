using MediatR;

namespace Prodify.Application.Sellers.Commands.RejectSeller;

public class RejectSellerCommand : IRequest
{
    public Guid SellerId { get; set; }

    // Shown to the seller, so write it for them.
    public string Reason { get; set; } = null!;
}