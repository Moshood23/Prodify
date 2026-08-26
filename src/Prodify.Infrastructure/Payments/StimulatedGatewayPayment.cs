using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Infrastructure.Payments;

public class SimulatedPaymentGateway : IPaymentService
{
    private const string ForceFailureToken = "FAIL";

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
}