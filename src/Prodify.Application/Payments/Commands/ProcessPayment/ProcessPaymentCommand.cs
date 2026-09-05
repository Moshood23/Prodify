using MediatR;

namespace Prodify.Application.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommand : IRequest<Guid>
{
    public Guid OrderId { get; set; }
    public string PaymentMethodToken { get; set; } = null!;
}