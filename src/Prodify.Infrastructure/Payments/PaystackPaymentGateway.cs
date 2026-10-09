using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Ordering.ValueObjects;

namespace Prodify.Infrastructure.Payments;

public class PaystackSettings
{
    // sk_test_... while testing, sk_live_... for real money. Found under Settings > API Keys & Webhooks.
    public string SecretKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.paystack.co";
}

// Customers pay on Paystack's own page, so card details never reach Prodify.
// Paystack works in kobo: 1 naira = 100 kobo.
public class PaystackPaymentGateway : IPaymentService
{
    private readonly HttpClient _http;
    private readonly string _secretKey;
    private readonly ILogger<PaystackPaymentGateway> _logger;

    public PaystackPaymentGateway(IOptions<PaystackSettings> options, ILogger<PaystackPaymentGateway> logger)
    {
        var settings = options.Value;
        _secretKey = settings.SecretKey.Trim();
        _logger = logger;
        _http = new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) })
        {
            BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(30)
        };
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
    }

    public string Provider => PaymentProviders.Paystack;

    public bool IsTestMode => _secretKey.StartsWith("sk_test_", StringComparison.Ordinal);

    public Task<PaymentResult> ChargeAsync(Money amount, string paymentMethodToken, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PaymentResult(false, null, "Card payments go through Paystack."));

    public async Task<string> StartHostedCheckoutAsync(HostedCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var body = new
        {
            email = request.Email,
            amount = ToKobo(request.Amount.Amount).ToString(CultureInfo.InvariantCulture),
            currency = request.Amount.Currency,
            reference = request.Reference,
            callback_url = request.CallbackUrl,
            metadata = new { orderId = request.OrderId }
        };

        var reply = await SendAsync(HttpMethod.Post, "transaction/initialize", body, cancellationToken);
        if (reply is null || !reply.Ok || !Has(reply.Data, "authorization_url", out var url) || url.GetString() is not { Length: > 0 } address)
        {
            _logger.LogWarning("Paystack did not start payment {Reference}: {Message}", request.Reference, reply?.Message);
            throw new BusinessRuleException("We couldn't open the Paystack payment page. Please try again in a moment.");
        }

        return address;
    }

    public async Task<GatewayTransaction> VerifyAsync(string reference, CancellationToken cancellationToken = default)
    {
        var reply = await SendAsync(HttpMethod.Get, $"transaction/verify/{Uri.EscapeDataString(reference)}", null, cancellationToken);
        if (reply is null)
            throw new BusinessRuleException("We couldn't reach Paystack to check your payment. Please try again in a moment.");

        if (!reply.Ok)
            return new GatewayTransaction(GatewayTransactionStatus.Pending, 0, reply.Message);

        var data = reply.Data;
        var status = Text(data, "status");
        var kobo = Has(data, "amount", out var amount) && amount.TryGetInt64(out var value) ? value : 0;
        var currency = Text(data, "currency");
        var naira = kobo / 100m;

        if (status == "success")
        {
            // Should never happen, but never take a payment in another currency as naira.
            if (!string.Equals(currency, "NGN", StringComparison.OrdinalIgnoreCase))
                return new GatewayTransaction(GatewayTransactionStatus.Failed, naira, $"Paid in {currency} instead of NGN.");

            return new GatewayTransaction(GatewayTransactionStatus.Succeeded, naira, null);
        }

        if (status is "failed" or "reversed")
            return new GatewayTransaction(GatewayTransactionStatus.Failed, naira, Text(data, "gateway_response") ?? "The payment was declined.");

        // "abandoned", "ongoing", "pending", "queued": not paid (yet).
        return new GatewayTransaction(GatewayTransactionStatus.Pending, naira, null);
    }

    public bool IsGenuineWebhook(string payload, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature) || _secretKey.Length == 0)
            return false;

        var expected = HMACSHA512.HashData(Encoding.UTF8.GetBytes(_secretKey), Encoding.UTF8.GetBytes(payload));

        byte[] given;
        try
        {
            given = Convert.FromHexString(signature.Trim());
        }
        catch (FormatException)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expected, given);
    }

    public async Task<PaymentResult> RefundAsync(string gatewayReference, Money amount, CancellationToken cancellationToken = default)
    {
        // Orders paid before Paystack was switched on used the simulated gateway; there is nothing to send back.
        if (gatewayReference.StartsWith("SIM-", StringComparison.Ordinal))
            return new PaymentResult(true, $"SIMREF-{Guid.NewGuid():N}", null);

        var body = new
        {
            transaction = gatewayReference,
            amount = ToKobo(amount.Amount).ToString(CultureInfo.InvariantCulture)
        };

        var reply = await SendAsync(HttpMethod.Post, "refund", body, cancellationToken);
        if (reply is null)
            return new PaymentResult(false, null, "Paystack could not be reached");

        if (!reply.Ok)
            return new PaymentResult(false, null, reply.Message ?? "Paystack refused the refund");

        // Paystack sends the money back over the next few days; the refund is already booked.
        var id = Has(reply.Data, "id", out var refundId) ? refundId.ToString() : null;
        return new PaymentResult(true, string.IsNullOrEmpty(id) ? $"REF-{gatewayReference}" : $"REF-{id}", null);
    }

    private static long ToKobo(decimal naira) => (long)Math.Round(naira * 100m, MidpointRounding.AwayFromZero);

    private static bool Has(JsonElement data, string name, out JsonElement value)
    {
        value = default;
        return data.ValueKind == JsonValueKind.Object && data.TryGetProperty(name, out value);
    }

    private static string? Text(JsonElement data, string name) =>
        Has(data, name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private sealed record Reply(bool Ok, string? Message, JsonElement Data);

    // Paystack answers every call with { status, message, data }. Null means it couldn't be reached.
    private async Task<Reply?> SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        try
        {
            using var message = new HttpRequestMessage(method, path);
            if (body is not null)
                message.Content = JsonContent.Create(body);

            using var response = await _http.SendAsync(message, cancellationToken);
            var text = await response.Content.ReadAsStringAsync(cancellationToken);

            using var json = JsonDocument.Parse(text);
            var root = json.RootElement;
            var ok = response.IsSuccessStatusCode && Has(root, "status", out var status) && status.ValueKind == JsonValueKind.True;
            var reason = Text(root, "message");
            var data = Has(root, "data", out var d) ? d.Clone() : default;

            if (!ok)
                _logger.LogWarning("Paystack {Method} {Path} answered {StatusCode}: {Message}", method, path, (int)response.StatusCode, reason);

            return new Reply(ok, reason, data);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            if (cancellationToken.IsCancellationRequested)
                throw;

            _logger.LogError(ex, "Paystack {Method} {Path} failed", method, path);
            return null;
        }
    }
}
