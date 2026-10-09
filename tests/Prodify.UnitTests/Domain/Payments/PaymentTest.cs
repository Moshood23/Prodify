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


    [Fact]
    public void RecordRefund_PartThenRest_EndsFullyRefunded()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));
        var attempt = payment.StartAttempt();
        payment.CompleteAttempt(attempt.Id, "gateway-ref-789");

        payment.RecordRefund(400m);
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
        Assert.Equal(400m, payment.RefundedAmount);

        payment.RecordRefund(600m);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal("gateway-ref-789", payment.GatewayReference);
    }

    [Fact]
    public void RecordRefund_MoreThanPaid_Throws()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));
        var attempt = payment.StartAttempt();
        payment.CompleteAttempt(attempt.Id, "gateway-ref-790");

        Assert.Throws<InvalidOperationException>(() => payment.RecordRefund(1000.01m));
    }

    [Fact]
    public void RecordRefund_BeforePaymentSucceeded_Throws()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));

        Assert.Throws<InvalidOperationException>(() => payment.RecordRefund(100m));
    }


    [Fact]
    public void StartAttempt_WithReference_KeepsItOnThePendingAttempt()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));

        var attempt = payment.StartAttempt("ORD-1-ABC");

        Assert.Equal("ORD-1-ABC", attempt.GatewayReference);
        Assert.Equal(PaymentAttemptStatus.Pending, attempt.Status);
        Assert.Null(payment.GatewayReference);
    }

    [Fact]
    public void FailAttempt_AfterAnotherAttemptSucceeded_KeepsThePaymentSucceeded()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));
        var first = payment.StartAttempt("ref-1");
        var second = payment.StartAttempt("ref-2");

        payment.CompleteAttempt(second.Id, "ref-2");
        payment.FailAttempt(first.Id, "Paid twice; sent back.");

        Assert.Equal(PaymentStatus.Succeeded, payment.Status);
        Assert.Equal("ref-2", payment.GatewayReference);
        Assert.Equal(PaymentAttemptStatus.Failed, first.Status);
    }

    [Fact]
    public void CompleteAttempt_WhenAlreadySucceeded_Throws()
    {
        var payment = Payment.Create(Guid.NewGuid(), Money.Create(1000m));
        var first = payment.StartAttempt("ref-1");
        var second = payment.StartAttempt("ref-2");
        payment.CompleteAttempt(first.Id, "ref-1");

        Assert.Throws<InvalidOperationException>(() => payment.CompleteAttempt(second.Id, "ref-2"));
    }
}