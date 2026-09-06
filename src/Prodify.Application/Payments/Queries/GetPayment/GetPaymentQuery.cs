using MediatR;

namespace Prodify.Application.Payments.Queries.GetPayment;

public class GetPaymentQuery : IRequest<PaymentDto>
{
    public Guid OrderId { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public int AttemptCount { get; set; }
}