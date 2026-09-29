using MediatR;

namespace Prodify.Application.Shipping.DeliveryFees.Commands.UpdateDeliveryFee;

// Admins change what delivery to a state costs. Orders already placed keep their fee.
public class UpdateDeliveryFeeCommand : IRequest
{
    public string State { get; set; } = null!;
    public decimal Fee { get; set; }
}
