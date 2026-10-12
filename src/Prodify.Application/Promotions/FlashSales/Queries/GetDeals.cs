using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Promotions.FlashSales.Common;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Application.Promotions.FlashSales.Queries;

// One flash sale on the deals page or the home page strip.
public class DealDto
{
    public Guid FlashSaleId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? VariantName { get; set; }
    public string? ImageUrl { get; set; }
    public decimal SalePrice { get; set; }
    public decimal WasPrice { get; set; }
    public int PercentOff { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int Quantity { get; set; }
    public int Sold { get; set; }
    public int MaxPerCustomer { get; set; }
    // False when the sale units are gone or the seller has no stock left.
    public bool CanBuy { get; set; }
}

public class DealsDto
{
    public List<DealDto> Live { get; set; } = new();
    // Approved sales starting in the next few days.
    public List<DealDto> Upcoming { get; set; } = new();
}

public class GetDealsQuery : IRequest<DealsDto>
{
}

public class GetDealsQueryHandler : IRequestHandler<GetDealsQuery, DealsDto>
{
    private const int MaxLive = 60;
    private const int MaxUpcoming = 20;
    private const int UpcomingDays = 3;

    private readonly IApplicationDbContext _context;

    public GetDealsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DealsDto> Handle(GetDealsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var live = await LoadAsync(_context.FlashSales.RunningAt(now), MaxLive, cancellationToken);

        var soon = now.AddDays(UpcomingDays);
        var upcoming = await LoadAsync(
            _context.FlashSales.Where(s => s.Status == FlashSaleStatus.Approved && s.StartsAt > now && s.StartsAt <= soon),
            MaxUpcoming, cancellationToken);

        var stock = await _context.GetAvailableStockAsync(live.Select(d => d.ProductVariantId).Distinct().ToList(), cancellationToken);
        foreach (var deal in live)
            deal.CanBuy = deal.Sold < deal.Quantity && stock.GetValueOrDefault(deal.ProductVariantId) > 0;

        return new DealsDto
        {
            // Ones you can still buy first, the ones ending soonest at the top.
            Live = live.OrderByDescending(d => d.CanBuy).ThenBy(d => d.EndsAt).ToList(),
            Upcoming = upcoming.OrderBy(d => d.StartsAt).ToList()
        };
    }

    // Only sales on products shoppers can see, still priced below the product's price today.
    private async Task<List<DealDto>> LoadAsync(IQueryable<FlashSale> sales, int limit, CancellationToken cancellationToken)
    {
        var visible = _context.Products.WherePubliclyVisible(_context);

        var rows = await sales
            .Join(_context.ProductVariants.Where(v => v.IsActive == true), s => s.ProductVariantId, v => v.Id, (s, v) => new { Sale = s, Variant = v })
            .Where(x => x.Sale.SalePrice < x.Variant.Price)
            .Join(visible, x => x.Variant.ProductId, p => p.Id, (x, p) => new
            {
                x.Sale.Id,
                ProductId = p.Id,
                VariantId = x.Variant.Id,
                ProductName = p.Name,
                VariantName = x.Variant.Name,
                ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                x.Sale.SalePrice,
                x.Sale.RegularPrice,
                x.Variant.Price,
                x.Sale.StartsAt,
                x.Sale.EndsAt,
                x.Sale.Quantity,
                x.Sale.SoldQuantity,
                x.Sale.MaxPerCustomer
            })
            .OrderBy(x => x.EndsAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return rows.Select(r =>
        {
            var was = Math.Min(r.Price, r.RegularPrice);
            return new DealDto
            {
                FlashSaleId = r.Id,
                ProductId = r.ProductId,
                ProductVariantId = r.VariantId,
                ProductName = r.ProductName,
                VariantName = r.VariantName,
                ImageUrl = r.ImageUrl,
                SalePrice = r.SalePrice,
                WasPrice = was,
                PercentOff = was > 0 ? (int)Math.Round((was - r.SalePrice) / was * 100m) : 0,
                StartsAt = r.StartsAt,
                EndsAt = r.EndsAt,
                Quantity = r.Quantity,
                Sold = r.SoldQuantity,
                MaxPerCustomer = r.MaxPerCustomer
            };
        }).ToList();
    }
}
