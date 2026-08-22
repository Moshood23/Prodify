using Prodify.Domain.Ordering.ValueObjects;
using Prodify.Domain.Payments.Entities;

namespace Prodify.UnitTests.Domain.Payments;

public class PaymentTests
{
    [Fact]
    public void Create_SetsInitialStatusPending()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Empty(payment.Attempts);
    }

    [Fact]
    public void StartAttempt_SetsStatusProcessing()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));
        payment.StartAttempt();

        Assert.Equal(PaymentStatus.Processing, payment.Status);
        Assert.Single(payment.Attempts);
    }

    [Fact]
    public void CompleteAttempt_SetsStatusSucceeded()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));
        var attempt = payment.StartAttempt();

        payment.CompleteAttempt(attempt.Id, "gateway-ref-123");

        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
    }

    [Fact]
    public void FailAttempt_SetsStatusFailed()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));
        var attempt = payment.StartAttempt();

        payment.FailAttempt(attempt.Id, "Insufficient funds");

        Assert.Equal(PaymentStatus.Failed, payment.Status);
    }

    [Fact]
    public void StartAttempt_AfterSucceeded_ThrowsInvalidOperationException()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));
        var attempt = payment.StartAttempt();
        payment.CompleteAttempt(attempt.Id, "gateway-ref-123");

        Assert.Throws<InvalidOperationException>(() => payment.StartAttempt());
    }

    [Fact]
    public void FailedAttempt_AllowsRetryWithNewAttempt()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));
        var firstAttempt = payment.StartAttempt();
        payment.FailAttempt(firstAttempt.Id, "Network error");

        var secondAttempt = payment.StartAttempt();
        payment.CompleteAttempt(secondAttempt.Id, "gateway-ref-456");

        Assert.Equal(2, payment.Attempts.Count);
        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
    }
}