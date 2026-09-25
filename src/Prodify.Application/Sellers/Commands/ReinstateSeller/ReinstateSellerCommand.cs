using MediatR;

namespace Prodify.Application.Sellers.Commands.ReinstateSeller;

public class ReinstateSellerCommand : IRequest
{
    public Guid SellerId { get; set; }
}