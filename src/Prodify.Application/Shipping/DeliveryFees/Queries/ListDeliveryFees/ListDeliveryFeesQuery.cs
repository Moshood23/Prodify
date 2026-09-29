using MediatR;

namespace Prodify.Application.Shipping.DeliveryFees.Queries.ListDeliveryFees;

// Every state's delivery fee, A to Z. Public: checkout shows the fee before ordering.
public class ListDeliveryFeesQuery : IRequest<List<DeliveryFeeDto>>
{
}

public class DeliveryFeeDto
{
    public string State { get; set; } = null!;
    public decimal Fee { get; set; }
}
