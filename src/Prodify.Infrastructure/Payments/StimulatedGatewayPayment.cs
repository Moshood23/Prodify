using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Infrastructure.Payments;

public class SimulatedPaymentGateway : IPaymentService
{
    private const string ForceFailureToken = "FAIL";

    public string Provider => PaymentProviders.Simulated;

    public bool IsTestMode => true;

    public Task<PaymentResult> ChargeAsync(Money amount, string paymentMethodToken, CancellationToken cancellationToken = default)
    {
        if (paymentMethodToken.Contains(ForceFailureToken, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new PaymentResult(
                Succeeded: false,
                GatewayReference: null,
                FailureReason: "Simulated payment failure."));
        }

        var gatewayReference = $"SIM-{Guid.NewGuid():N}";

        return Task.FromResult(new PaymentResult(
            Succeeded: true,
            GatewayReference: gatewayReference,
            FailureReason: null));
    }

    public Task<PaymentResult> RefundAsync(string gatewayReference, Money amount, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PaymentResult(
            Succeeded: true,
            GatewayReference: $"SIMREF-{Guid.NewGuid():N}",
            FailureReason: null));

    public Task<string> StartHostedCheckoutAsync(HostedCheckoutRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The simulated gateway has no payment page.");

    public Task<GatewayTransaction> VerifyAsync(string reference, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The simulated gateway has no payment page.");

    public bool IsGenuineWebhook(string payload, string? signature) => false;
}
