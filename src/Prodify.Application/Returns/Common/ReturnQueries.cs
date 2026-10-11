using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Returns.Entities;

namespace Prodify.Application.Returns.Common;

public static class ReturnQueries
{
    // Loads the returns with their items, order numbers and seller names, newest first.
    public static async Task<List<ReturnDto>> ToReturnDtosAsync(
        this IApplicationDbContext context, IQueryable<ReturnRequest> returns, int take, CancellationToken cancellationToken)
    {
        var list = await returns
            .Include(r => r.Items)
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var orderIds = list.Select(r => r.OrderId).Distinct().ToList();
        var orders = await context.Orders
            .Where(o => orderIds.Contains(o.Id))
            .Select(o => new { o.Id, Number = o.OrderNumber.Value, o.ShippingAddress.RecipientName, o.PaymentMethod })
            .ToDictionaryAsync(o => o.Id, cancellationToken);

        var sellerIds = list.Select(r => r.SellerId).Distinct().ToList();
        var sellers = await context.Sellers
            .Where(s => sellerIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.BusinessName, cancellationToken);

        return list.Select(r => new ReturnDto
        {
            Id = r.Id,
            OrderId = r.OrderId,
            OrderNumber = orders.GetValueOrDefault(r.OrderId)?.Number ?? "",
            SellerOrderId = r.SellerOrderId,
            SellerName = sellers.GetValueOrDefault(r.SellerId) ?? "Seller",
            CustomerName = orders.GetValueOrDefault(r.OrderId)?.RecipientName ?? "",
            PaymentMethod = orders.GetValueOrDefault(r.OrderId)?.PaymentMethod.ToString() ?? "",
            Reason = r.Reason.ToString(),
            Details = r.Details,
            Status = r.Status.ToString(),
            SellerNote = r.SellerNote,
            CustomerNote = r.CustomerNote,
            AdminNote = r.AdminNote,
            RefundAmount = r.RefundAmount,
            RefundedToCard = r.RefundedToCard,
            RefundedAsCredit = r.RefundedAsCredit,
            CreatedAt = r.CreatedAt,
            ResolvedAt = r.ResolvedAt,
            Items = r.Items.Select(i => new ReturnItemDto
            {
                ProductVariantId = i.ProductVariantId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        }).ToList();
    }

    public static async Task<ReturnDto?> GetReturnDtoAsync(
        this IApplicationDbContext context, Guid sellerOrderId, CancellationToken cancellationToken) =>
        (await context.ToReturnDtosAsync(context.ReturnRequests.Where(r => r.SellerOrderId == sellerOrderId), 1, cancellationToken))
            .FirstOrDefault();
}
