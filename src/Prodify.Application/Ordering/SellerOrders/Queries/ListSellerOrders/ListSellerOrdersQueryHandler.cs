using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Models;
using Prodify.Application.Common.Security;
using Prodify.Application.Ordering.SellerOrders.Common;
using Prodify.Domain.Ordering.Entities;

namespace Prodify.Application.Ordering.SellerOrders.Queries.ListSellerOrders;

public class ListSellerOrdersQueryHandler : IRequestHandler<ListSellerOrdersQuery, PaginatedList<SellerOrderSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ListSellerOrdersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<SellerOrderSummaryDto>> Handle(ListSellerOrdersQuery request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.IsAdmin()
            ? request.SellerId ?? throw new BusinessRuleException("Choose a seller to see their orders.")
            : _currentUser.GetRequiredSellerId();

        var rows = _context.SellerOrders
            .Where(so => so.SellerId == sellerId)
            .Join(_context.Orders, so => so.OrderId, o => o.Id, (so, o) => new { SellerOrder = so, Order = o })
            // Unpaid card orders are not real orders yet.
            .Where(x => x.Order.PaymentMethod == PaymentMethod.PayOnDelivery || x.Order.IsPaid);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = Enum.Parse<SellerOrderStatus>(request.Status, ignoreCase: true);
            rows = rows.Where(x => x.SellerOrder.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            rows = rows.Where(x => x.Order.OrderNumber.Value.Contains(search));
        }

        var totalCount = await rows.CountAsync(cancellationToken);

        var page = await rows
            .OrderByDescending(x => x.Order.CreatedAt)
            .ThenBy(x => x.SellerOrder.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new
            {
                x.SellerOrder.Id,
                x.SellerOrder.OrderId,
                OrderNumber = x.Order.OrderNumber.Value,
                x.Order.CreatedAt,
                x.SellerOrder.Status,
                x.Order.PaymentMethod,
                x.Order.IsPaid,
                x.Order.ShippingAddress.RecipientName,
                x.Order.ShippingAddress.City,
                x.Order.ShippingAddress.State,
                Items = x.SellerOrder.Items
                    .Select(i => new { i.ProductVariantId, i.ProductName, i.Quantity, Price = i.UnitPrice.Amount })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var firstVariantIds = page
            .Select(p => p.Items.FirstOrDefault()?.ProductVariantId)
            .OfType<Guid>()
            .Distinct()
            .ToList();

        var images = await _context.ProductVariants
            .Where(v => firstVariantIds.Contains(v.Id))
            .Join(_context.Products, v => v.ProductId, p => p.Id, (v, p) => new
            {
                VariantId = v.Id,
                ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.VariantId, x => x.ImageUrl, cancellationToken);

        var items = page.Select(p =>
        {
            var first = p.Items.FirstOrDefault();
            var others = p.Items.Count - 1;

            return new SellerOrderSummaryDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                OrderNumber = p.OrderNumber,
                CreatedAt = p.CreatedAt,
                Status = p.Status.ToString(),
                PaymentMethod = p.PaymentMethod.ToString(),
                IsPaid = p.IsPaid,
                CustomerName = p.RecipientName,
                City = p.City,
                State = p.State,
                ItemCount = p.Items.Sum(i => i.Quantity),
                Summary = first is null ? "" : others > 0 ? $"{first.ProductName} and {others} more" : first.ProductName,
                ImageUrl = first is null ? null : images.GetValueOrDefault(first.ProductVariantId),
                Total = p.Items.Sum(i => i.Price * i.Quantity)
            };
        }).ToList();

        return new PaginatedList<SellerOrderSummaryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}