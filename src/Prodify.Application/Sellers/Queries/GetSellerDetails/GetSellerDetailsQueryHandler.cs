using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Sellers.Common;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Sellers.Queries.GetSellerDetails;

public class GetSellerDetailsQueryHandler : IRequestHandler<GetSellerDetailsQuery, SellerDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetSellerDetailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SellerDetailsDto> Handle(GetSellerDetailsQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.Sellers
            .Where(s => s.Id == request.Id)
            .Select(SellerProjections.ToProfile)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Seller", request.Id);

        var sellerOrders = _context.SellerOrders
            .Where(so => so.SellerId == request.Id && so.Status != SellerOrderStatus.Cancelled);

        return new SellerDetailsDto
        {
            Profile = profile,
            ProductCount = await _context.Products.CountAsync(p => p.SellerId == request.Id, cancellationToken),
            ActiveProductCount = await _context.Products.CountAsync(p => p.SellerId == request.Id && p.IsActive, cancellationToken),
            OrderCount = await sellerOrders.CountAsync(cancellationToken),
            TotalSales = await sellerOrders
                .SelectMany(so => so.Items)
                .SumAsync(i => (decimal?)(i.UnitPrice.Amount * i.Quantity), cancellationToken) ?? 0
        };
    }
}