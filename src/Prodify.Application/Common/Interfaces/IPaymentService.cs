using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Application.Common.Interfaces;

public record PaymentResult(bool Succeeded, string? GatewayReference, string? FailureReason);

// Paystack: the customer pays on Paystack's own page, then comes back to the website.
public record HostedCheckoutRequest(string Reference, Money Amount, string Email, string CallbackUrl, Guid OrderId);

public enum GatewayTransactionStatus
{
    // Not finished yet: the customer is still on the payment page, or left it without paying.
    Pending,
    Succeeded,
    Failed
}

public record GatewayTransaction(GatewayTransactionStatus Status, decimal Amount, string? FailureReason);

public static class PaymentProviders
{
    // The built-in test card form; no real money moves.
    public const string Simulated = "Simulated";
    public const string Paystack = "Paystack";
}

public interface IPaymentService
{
    // One of PaymentProviders.
    string Provider { get; }

    // True while no real money can move (simulated gateway, or a Paystack sk_test_ key).
    bool IsTestMode { get; }

    // Simulated gateway only: charges a card token straight away.
    Task<PaymentResult> ChargeAsync(Money amount, string paymentMethodToken, CancellationToken cancellationToken = default);

    // Paystack only: returns the address of the payment page to send the customer to.
    Task<string> StartHostedCheckoutAsync(HostedCheckoutRequest request, CancellationToken cancellationToken = default);

    // Paystack only: asks the gateway how a payment went. Never trust what the browser says.
    Task<GatewayTransaction> VerifyAsync(string reference, CancellationToken cancellationToken = default);

    // Paystack only: true when a webhook really came from the gateway.
    bool IsGenuineWebhook(string payload, string? signature);

    // Sends money back for an earlier successful charge.
    Task<PaymentResult> RefundAsync(string gatewayReference, Money amount, CancellationToken cancellationToken = default);
}
