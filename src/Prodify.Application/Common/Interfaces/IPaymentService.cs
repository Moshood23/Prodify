using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Application.Common.Interfaces;

public record PaymentResult(bool Succeeded, string? GatewayReference, string? FailureReason);

public interface IPaymentService
{
    Task<PaymentResult> ChargeAsync(Money amount, string paymentMethodToken, CancellationToken cancellationToken = default);
}