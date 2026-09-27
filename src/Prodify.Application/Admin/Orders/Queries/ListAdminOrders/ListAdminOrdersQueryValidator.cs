using FluentValidation;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Admin.Orders.Queries.ListAdminOrders;

public class ListAdminOrdersQueryValidator : AbstractValidator<ListAdminOrdersQuery>
{
    public static readonly string[] Stages = { "awaiting_payment", "in_progress", "delivered", "cancelled" };

    public ListAdminOrdersQueryValidator()
    {
        RuleFor(x => x.Stage)
            .Must(stage => Stages.Contains(stage!.ToLowerInvariant()))
            .When(x => !string.IsNullOrWhiteSpace(x.Stage))
            .WithMessage($"Stage must be one of: {string.Join(", ", Stages)}.");

        RuleFor(x => x.PaymentMethod)
            .Must(method => Enum.TryParse<PaymentMethod>(method, ignoreCase: true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.PaymentMethod))
            .WithMessage("Payment method must be Card or PayOnDelivery.");

        RuleFor(x => x.Search)
            .MaximumLength(100);

        RuleFor(x => x)
            .Must(x => x.From <= x.To)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithName("To")
            .WithMessage("The end date must be on or after the start date.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);
    }
}