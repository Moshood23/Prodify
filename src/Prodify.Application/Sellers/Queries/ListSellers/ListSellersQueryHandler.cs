using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;
using Prodify.Domain.Sellers.Entities;

namespace Prodify.Application.Sellers.Queries.ListSellers;

public class ListSellersQueryHandler : IRequestHandler<ListSellersQuery, PaginatedList<SellerSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public ListSellersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<SellerSummaryDto>> Handle(ListSellersQuery request, CancellationToken cancellationToken)
    {
        var sellers = _context.Sellers.AsQueryable();

        if (Enum.TryParse<SellerStatus>(request.Status, ignoreCase: true, out var status))
            sellers = sellers.Where(s => s.Status == status);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            sellers = sellers.Where(s => s.BusinessName.Contains(term) || s.Email.Contains(term));
        }

        var totalCount = await sellers.CountAsync(cancellationToken);

        // Applications waiting longest come first; otherwise newest first.
        var ordered = status == SellerStatus.PendingVerification
            ? sellers.OrderBy(s => s.CreatedAt)
            : sellers.OrderByDescending(s => s.CreatedAt);

        var items = await ordered
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SellerSummaryDto
            {
                Id = s.Id,
                BusinessName = s.BusinessName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Status = s.Status.ToString(),
                City = s.Addresses.Select(a => a.City).FirstOrDefault(),
                State = s.Addresses.Select(a => a.State).FirstOrDefault(),
                CreatedAt = s.CreatedAt,
                StatusChangedAt = s.StatusChangedAt,
                ProductCount = _context.Products.Count(p => p.SellerId == s.Id)
            })
            .ToListAsync(cancellationToken);

        return new PaginatedList<SellerSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}