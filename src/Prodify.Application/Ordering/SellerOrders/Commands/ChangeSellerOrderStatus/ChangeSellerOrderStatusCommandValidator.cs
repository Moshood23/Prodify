using FluentValidation;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.SellerOrders.Commands.ChangeSellerOrderStatus;

public class ChangeSellerOrderStatusCommandValidator : AbstractValidator<ChangeSellerOrderStatusCommand>
{
    public static readonly string[] Statuses =
    {
        nameof(SellerOrderStatus.Confirmed),
        nameof(SellerOrderStatus.Packed),
        nameof(SellerOrderStatus.Shipped),
        nameof(SellerOrderStatus.Delivered),
        nameof(SellerOrderStatus.Cancelled)
    };

    public ChangeSellerOrderStatusCommandValidator()
    {
        RuleFor(x => x.SellerOrderId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => Statuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", Statuses)}.");

        RuleFor(x => x.Carrier)
            .NotEmpty()
            .WithMessage("Enter who is delivering the package.")
            .MaximumLength(100)
            .When(x => IsStatus(x, SellerOrderStatus.Shipped));

        RuleFor(x => x.TrackingNumber)
            .MaximumLength(100);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Tell the customer why the order is cancelled.")
            .MaximumLength(500)
            .When(x => IsStatus(x, SellerOrderStatus.Cancelled));
    }

    private static bool IsStatus(ChangeSellerOrderStatusCommand command, SellerOrderStatus status) =>
        string.Equals(command.Status, status.ToString(), StringComparison.OrdinalIgnoreCase);
}