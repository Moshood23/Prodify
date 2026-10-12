using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Promotions.Entities;

namespace Prodify.Application.Promotions.FlashSales.Common;

// A flash sale as its seller and Prodify's staff see it.
public class FlashSaleDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? VariantName { get; set; }
    public string? ImageUrl { get; set; }

    public decimal SalePrice { get; set; }
    // The price when the sale was set up, and the price today.
    public decimal RegularPrice { get; set; }
    public decimal CurrentPrice { get; set; }

    public int Quantity { get; set; }
    public int Sold { get; set; }
    public int MaxPerCustomer { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    // Pending, Approved, Rejected, Cancelled or Stopped: what was decided.
    public string Status { get; set; } = null!;
    // Where it is now: Pending, Expired (never reviewed in time), Upcoming, Live, SoldOut, Ended, Rejected, Cancelled or Stopped.
    public string State { get; set; } = null!;
    public string? ReviewNote { get; set; }
    public DateTime CreatedAt { get; set; }
}

public static class FlashSaleQueries
{
    public static string StateOf(FlashSaleStatus status, DateTime startsAt, DateTime endsAt, int quantity, int sold, DateTime now) =>
        status switch
        {
            FlashSaleStatus.Pending => endsAt <= now ? "Expired" : "Pending",
            FlashSaleStatus.Approved when endsAt <= now => "Ended",
            FlashSaleStatus.Approved when startsAt > now => "Upcoming",
            FlashSaleStatus.Approved when sold >= quantity => "SoldOut",
            FlashSaleStatus.Approved => "Live",
            _ => status.ToString()
        };

    public static async Task<List<FlashSaleDto>> ToFlashSaleDtosAsync(
        this IApplicationDbContext context, IQueryable<FlashSale> sales, int limit, DateTime now, CancellationToken cancellationToken)
    {
        var rows = await sales
            .OrderByDescending(s => s.CreatedAt)
            .Take(limit)
            .Join(context.ProductVariants, s => s.ProductVariantId, v => v.Id, (s, v) => new { Sale = s, Variant = v })
            .Join(context.Products, x => x.Variant.ProductId, p => p.Id, (x, p) => new
            {
                x.Sale,
                ProductId = p.Id,
                ProductName = p.Name,
                VariantName = x.Variant.Name,
                CurrentPrice = x.Variant.Price,
                ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
                SellerName = context.Sellers.Where(se => se.Id == p.SellerId).Select(se => se.BusinessName).FirstOrDefault() ?? ""
            })
            .ToListAsync(cancellationToken);

        return rows
            .OrderByDescending(r => r.Sale.CreatedAt)
            .Select(r => new FlashSaleDto
            {
                Id = r.Sale.Id,
                SellerId = r.Sale.SellerId,
                SellerName = r.SellerName,
                ProductId = r.ProductId,
                ProductVariantId = r.Sale.ProductVariantId,
                ProductName = r.ProductName,
                VariantName = r.VariantName,
                ImageUrl = r.ImageUrl,
                SalePrice = r.Sale.SalePrice,
                RegularPrice = r.Sale.RegularPrice,
                CurrentPrice = r.CurrentPrice,
                Quantity = r.Sale.Quantity,
                Sold = r.Sale.SoldQuantity,
                MaxPerCustomer = r.Sale.MaxPerCustomer,
                StartsAt = r.Sale.StartsAt,
                EndsAt = r.Sale.EndsAt,
                Status = r.Sale.Status.ToString(),
                State = StateOf(r.Sale.Status, r.Sale.StartsAt, r.Sale.EndsAt, r.Sale.Quantity, r.Sale.SoldQuantity, now),
                ReviewNote = r.Sale.ReviewNote,
                CreatedAt = r.Sale.CreatedAt
            })
            .ToList();
    }
}

// What a product page or cart line shows about a flash sale running on the variant.
public class FlashSaleOfferDto
{
    public Guid FlashSaleId { get; set; }
    public decimal SalePrice { get; set; }
    public decimal WasPrice { get; set; }
    public DateTime EndsAt { get; set; }
    public int Quantity { get; set; }
    public int Sold { get; set; }
    public int MaxPerCustomer { get; set; }

    // How many more this shopper can have at the sale price (0 = sold out, or they've had their share).
    public int Allowance { get; set; }

    public static FlashSaleOfferDto From(FlashSaleOffer offer) => new()
    {
        FlashSaleId = offer.FlashSaleId,
        SalePrice = offer.SalePrice,
        WasPrice = offer.WasPrice,
        EndsAt = offer.EndsAt,
        Quantity = offer.Quantity,
        Sold = offer.Sold,
        MaxPerCustomer = offer.MaxPerCustomer,
        Allowance = offer.Allowance
    };
}
