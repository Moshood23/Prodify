using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Notifications.Entities;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Application.Promotions.FlashSales.Commands;

// Prodify's staff decide on the flash sales sellers set up, and can end one early.

public class ApproveFlashSaleCommand : IRequest
{
    public Guid FlashSaleId { get; set; }
}

public class ApproveFlashSaleCommandHandler : IRequestHandler<ApproveFlashSaleCommand>
{
    private readonly IApplicationDbContext _context;

    public ApproveFlashSaleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ApproveFlashSaleCommand request, CancellationToken cancellationToken)
    {
        var (sale, productName) = await FlashSaleDecisions.LoadAsync(_context, request.FlashSaleId, cancellationToken);

        // The seller may have changed the price since: the sale must still be a real discount.
        var price = await _context.ProductVariants.Where(v => v.Id == sale.ProductVariantId).Select(v => v.Price).FirstAsync(cancellationToken);
        if (sale.SalePrice > FlashSale.MaxSalePriceFor(price))
            throw new BusinessRuleException("The seller has changed the product's price, so this is no longer a real discount. Reject it instead.");

        var now = DateTime.UtcNow;
        sale.Approve(now);

        var when = sale.StartsAt <= now ? "It is live now." : "It goes live at the time you chose.";
        _context.Add(Notification.Create(sale.SellerId, NotificationType.FlashSale, "Your flash sale is approved",
            $"The flash sale on {productName} is approved. {when}", FlashSaleDecisions.SellerPage));
    }
}

public class RejectFlashSaleCommand : IRequest
{
    public Guid FlashSaleId { get; set; }
    public string Note { get; set; } = null!;
}

public class RejectFlashSaleCommandValidator : AbstractValidator<RejectFlashSaleCommand>
{
    public RejectFlashSaleCommandValidator()
    {
        RuleFor(x => x.Note).NotEmpty().WithMessage("Tell the seller why.").MaximumLength(FlashSale.MaxNoteLength);
    }
}

public class RejectFlashSaleCommandHandler : IRequestHandler<RejectFlashSaleCommand>
{
    private readonly IApplicationDbContext _context;

    public RejectFlashSaleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RejectFlashSaleCommand request, CancellationToken cancellationToken)
    {
        var (sale, productName) = await FlashSaleDecisions.LoadAsync(_context, request.FlashSaleId, cancellationToken);
        sale.Reject(request.Note, DateTime.UtcNow);

        _context.Add(Notification.Create(sale.SellerId, NotificationType.FlashSale, "Your flash sale was not approved",
            $"The flash sale on {productName} was not approved: {sale.ReviewNote}", FlashSaleDecisions.SellerPage));
    }
}

public class StopFlashSaleCommand : IRequest
{
    public Guid FlashSaleId { get; set; }
    public string Note { get; set; } = null!;
}

public class StopFlashSaleCommandValidator : AbstractValidator<StopFlashSaleCommand>
{
    public StopFlashSaleCommandValidator()
    {
        RuleFor(x => x.Note).NotEmpty().WithMessage("Tell the seller why.").MaximumLength(FlashSale.MaxNoteLength);
    }
}

public class StopFlashSaleCommandHandler : IRequestHandler<StopFlashSaleCommand>
{
    private readonly IApplicationDbContext _context;

    public StopFlashSaleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(StopFlashSaleCommand request, CancellationToken cancellationToken)
    {
        var (sale, productName) = await FlashSaleDecisions.LoadAsync(_context, request.FlashSaleId, cancellationToken);
        sale.Stop(request.Note, DateTime.UtcNow);

        _context.Add(Notification.Create(sale.SellerId, NotificationType.FlashSale, "Your flash sale was stopped",
            $"Prodify ended the flash sale on {productName} early: {sale.ReviewNote}", FlashSaleDecisions.SellerPage));
    }
}

internal static class FlashSaleDecisions
{
    public const string SellerPage = "/seller/flash-sales";

    public static async Task<(FlashSale Sale, string ProductName)> LoadAsync(
        IApplicationDbContext context, Guid id, CancellationToken cancellationToken)
    {
        var sale = await context.FlashSales.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(FlashSale), id);

        var productName = await context.ProductVariants
            .Where(v => v.Id == sale.ProductVariantId)
            .Join(context.Products, v => v.ProductId, p => p.Id, (v, p) => v.Name == null ? p.Name : p.Name + " (" + v.Name + ")")
            .FirstOrDefaultAsync(cancellationToken) ?? "your product";

        return (sale, productName);
    }
}
