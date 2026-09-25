using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Sellers.Common;

public static class SellerLookup
{
    public static async Task<Seller> GetSellerOrThrowAsync(
        this IApplicationDbContext context, Guid sellerId, CancellationToken cancellationToken) =>
        await context.Sellers.FirstOrDefaultAsync(s => s.Id == sellerId, cancellationToken)
        ?? throw new NotFoundException("Seller", sellerId);
}