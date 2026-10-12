using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Cart.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Application.Promotions.FlashSales.Commands;

// A seller puts one of their products on flash sale. It waits for Prodify to approve it.
public class CreateFlashSaleCommand : IRequest<Guid>
{
    public Guid ProductVariantId { get; set; }
    public decimal SalePrice { get; set; }
    public int Quantity { get; set; }
    public int MaxPerCustomer { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
}

public class CreateFlashSaleCommandValidator : AbstractValidator<CreateFlashSaleCommand>
{
    public CreateFlashSaleCommandValidator()
    {
        RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Choose the product to put on sale.");
        RuleFor(x => x.SalePrice).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, FlashSale.MaxUnits);
        RuleFor(x => x.MaxPerCustomer).InclusiveBetween(1, CartRules.MaxQuantityPerItem);
        RuleFor(x => x.EndsAt).GreaterThan(x => x.StartsAt).WithMessage("The sale must end after it starts.");
    }
}

public class CreateFlashSaleCommandHandler : IRequestHandler<CreateFlashSaleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateFlashSaleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateFlashSaleCommand request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.GetRequiredSellerId();

        var variant = await _context.ProductVariants
            .Where(v => v.Id == request.ProductVariantId && v.IsActive == true)
            .Where(v => _context.Products.Any(p => p.Id == v.ProductId && p.SellerId == sellerId))
            .Select(v => new { v.Id, v.Price })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductVariantId);

        var startsAt = request.StartsAt.ToUniversalTime();
        var endsAt = request.EndsAt.ToUniversalTime();

        var overlaps = await _context.FlashSales.AnyAsync(s =>
            s.ProductVariantId == variant.Id
            && (s.Status == FlashSaleStatus.Pending || s.Status == FlashSaleStatus.Approved)
            && s.StartsAt < endsAt
            && s.EndsAt > startsAt, cancellationToken);

        if (overlaps)
            throw new BusinessRuleException("This product already has a flash sale at that time. Pick other dates or cancel the other sale.");

        var sale = FlashSale.Create(sellerId, variant.Id, variant.Price, request.SalePrice, request.Quantity, request.MaxPerCustomer,
            startsAt, endsAt, DateTime.UtcNow, CartRules.MaxQuantityPerItem);

        _context.Add(sale);
        return sale.Id;
    }
}

// The seller withdraws a sale that hasn't ended yet. Units already sold stay sold.
public class CancelFlashSaleCommand : IRequest
{
    public Guid FlashSaleId { get; set; }
}

public class CancelFlashSaleCommandHandler : IRequestHandler<CancelFlashSaleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CancelFlashSaleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelFlashSaleCommand request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.GetRequiredSellerId();
        var sale = await _context.FlashSales.FirstOrDefaultAsync(s => s.Id == request.FlashSaleId && s.SellerId == sellerId, cancellationToken)
            ?? throw new NotFoundException(nameof(FlashSale), request.FlashSaleId);

        sale.Cancel(DateTime.UtcNow);
    }
}
