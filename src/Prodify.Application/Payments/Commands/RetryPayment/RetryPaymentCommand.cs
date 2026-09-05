using MediatR;

namespace Prodify.Application.Payments.Commands.RetryPayment;

public class RetryPaymentCommand : IRequest
{
    public Guid PaymentId { get; set; }
    public string PaymentMethodToken { get; set; } = null!;
}