using MediatR;

namespace Prodify.Application.Sellers.Commands.ApproveSeller;

public class ApproveSellerCommand : IRequest
{
    public Guid SellerId { get; set; }
}