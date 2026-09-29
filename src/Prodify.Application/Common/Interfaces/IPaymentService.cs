using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Application.Common.Interfaces;

public record PaymentResult(bool Succeeded, string? GatewayReference, string? FailureReason);

public interface IPaymentService
{
    Task<PaymentResult> ChargeAsync(Money amount, string paymentMethodToken, CancellationToken cancellationToken = default);

    // Sends money back for an earlier successful charge.
    Task<PaymentResult> RefundAsync(string gatewayReference, Money amount, CancellationToken cancellationToken = default);
}