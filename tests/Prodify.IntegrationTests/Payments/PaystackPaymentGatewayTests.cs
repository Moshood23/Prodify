using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Prodify.Infrastructure.Payments;

namespace Prodify.IntegrationTests.Payments;

public class PaystackPaymentGatewayTests
{
    private const string SecretKey = "sk_test_0123456789abcdef";
    private const string Payload = "{\"event\":\"charge.success\",\"data\":{\"reference\":\"ORD-20261009-ABCDEF12-1234ABCD\"}}";

    private static PaystackPaymentGateway Gateway() =>
        new(Options.Create(new PaystackSettings { SecretKey = SecretKey }), NullLogger<PaystackPaymentGateway>.Instance);

    private static string Sign(string payload, string key) =>
        Convert.ToHexString(HMACSHA512.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();

    [Fact]
    public void Accepts_a_webhook_signed_with_the_secret_key()
    {
        Assert.True(Gateway().IsGenuineWebhook(Payload, Sign(Payload, SecretKey)));
    }

    [Fact]
    public void Rejects_a_webhook_signed_with_another_key()
    {
        Assert.False(Gateway().IsGenuineWebhook(Payload, Sign(Payload, "sk_test_someone_else")));
    }

    [Fact]
    public void Rejects_a_webhook_whose_body_was_changed()
    {
        var signature = Sign(Payload, SecretKey);

        Assert.False(Gateway().IsGenuineWebhook(Payload.Replace("ABCDEF12", "ZZZZZZZZ"), signature));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-hex")]
    public void Rejects_a_webhook_without_a_valid_signature(string? signature)
    {
        Assert.False(Gateway().IsGenuineWebhook(Payload, signature));
    }
}
