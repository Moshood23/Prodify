using FluentValidation;
using MediatR;

namespace Prodify.Application.Payments.Commands.StartPaystackPayment;

// Opens a Paystack payment for a card order. The website sends the customer to AuthorizationUrl.
public class StartPaystackPaymentCommand : IRequest<StartPaystackPaymentResult>
{
    public Guid OrderId { get; set; }
}

public class StartPaystackPaymentResult
{
    public string AuthorizationUrl { get; set; } = null!;
    public string Reference { get; set; } = null!;
}

public class StartPaystackPaymentCommandValidator : AbstractValidator<StartPaystackPaymentCommand>
{
    public StartPaystackPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
