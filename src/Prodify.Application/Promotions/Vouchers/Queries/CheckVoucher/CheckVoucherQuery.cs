using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Cart.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Promotions.Vouchers.Common;

namespace Prodify.Application.Promotions.Vouchers.Queries.CheckVoucher;

// Checkout's "Apply" button: is the code good for the customer's cart, and how much does it take off?
// Nothing is saved; the code is checked again when the order is placed.
public class CheckVoucherQuery : IRequest<VoucherCheckDto>
{
    public string Code { get; set; } = null!;
}

public class VoucherCheckDto
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Discount { get; set; }
}

public class CheckVoucherQueryHandler : IRequestHandler<CheckVoucherQuery, VoucherCheckDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CheckVoucherQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VoucherCheckDto> Handle(CheckVoucherQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        var items = await _context.Carts
            .Where(c => c.CustomerId == customerId)
            .SelectMany(c => c.Items)
            .Select(i => new { i.ProductVariantId, i.Quantity })
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
            throw new BusinessRuleException("Your cart is empty.");

        // Today's prices, like the order itself.
        var variants = await _context.GetCartVariantsAsync(items.Select(i => i.ProductVariantId).ToList(), cancellationToken);
        var itemsTotal = items
            .Where(i => variants.TryGetValue(i.ProductVariantId, out var v) && v.IsAvailable)
            .Sum(i => variants[i.ProductVariantId].Price * i.Quantity);

        var (voucher, discount) = await _context.GetUsableVoucherAsync(request.Code, itemsTotal, cancellationToken);

        return new VoucherCheckDto { Code = voucher.Code, Description = voucher.Description, Discount = discount };
    }
}
