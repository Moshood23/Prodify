using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Sellers.Queries.GetSeller;

public class GetSellerQueryHandler : IRequestHandler<GetSellerQuery, PublicSellerDto>
{
    private readonly IApplicationDbContext _context;

    public GetSellerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PublicSellerDto> Handle(GetSellerQuery request, CancellationToken cancellationToken)
    {
        // Only approved stores are public; the rest look like they don't exist.
        var seller = await _context.Sellers
            .Where(s => s.Id == request.Id && s.Status == SellerStatus.Approved)
            .Select(s => new PublicSellerDto
            {
                Id = s.Id,
                BusinessName = s.BusinessName,
                Description = s.Description,
                JoinedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Seller", request.Id);

        seller.ProductCount = await _context.Products
            .Where(p => p.SellerId == seller.Id)
            .WherePubliclyVisible(_context)
            .CountAsync(cancellationToken);

        return seller;
    }
}