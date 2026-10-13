using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Catalog.Products.DTOs;
using Prodify.Application.Catalog.Products.Queries.ListProducts;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Domain.Customers.Entities;

namespace Prodify.Application.Catalog.Products.Recommendations;

public static class ProductCards
{
    // Product cards for these ids, in the same order. Hidden products are left out.
    public static async Task<List<ProductSummaryDto>> InOrderAsync(this ISender sender, IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return new List<ProductSummaryDto>();

        var page = await sender.Send(new ListProductsQuery { Ids = ids.ToList(), PageSize = ids.Count }, cancellationToken);
        var position = ids.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);

        return page.Items.OrderBy(p => position[p.Id]).ToList();
    }
}

// ---------- Recently viewed ----------

public class ViewedProduct
{
    public Guid ProductId { get; set; }

    // When the browser saw it (for lists kept while logged out). Empty means now.
    public DateTime? ViewedAt { get; set; }
}

// Saves products the customer looked at: one when they open a product page, or the list
// kept in the browser while they were logged out.
public class RecordProductViewsCommand : IRequest
{
    public List<ViewedProduct> Products { get; set; } = new();
}

public class RecordProductViewsCommandValidator : AbstractValidator<RecordProductViewsCommand>
{
    public RecordProductViewsCommandValidator()
    {
        RuleFor(x => x.Products)
            .NotEmpty()
            .WithMessage("Send at least one product.")
            .Must(p => p.Count <= ProductView.MaxPerCustomer)
            .WithMessage($"Send at most {ProductView.MaxPerCustomer} products.");

        RuleForEach(x => x.Products).ChildRules(p => p.RuleFor(v => v.ProductId).NotEmpty());
    }
}

public class RecordProductViewsCommandHandler : IRequestHandler<RecordProductViewsCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RecordProductViewsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(RecordProductViewsCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();
        var now = DateTime.UtcNow;

        // Two tabs saving at once must not both add the same product.
        await _context.LockCustomerAsync(customerId, cancellationToken);

        var ids = request.Products.Select(p => p.ProductId).Distinct().ToList();
        var known = await _context.Products.Where(p => ids.Contains(p.Id)).Select(p => p.Id).ToListAsync(cancellationToken);

        var views = await _context.ProductViews.Where(v => v.CustomerId == customerId).ToListAsync(cancellationToken);

        foreach (var item in request.Products.Where(p => known.Contains(p.ProductId)))
        {
            // A time from the browser can't be in the future.
            var viewedAt = item.ViewedAt is { } at && at.ToUniversalTime() < now ? at.ToUniversalTime() : now;

            var view = views.FirstOrDefault(v => v.ProductId == item.ProductId);
            if (view is null)
            {
                view = ProductView.Create(customerId, item.ProductId, viewedAt);
                _context.Add(view);
                views.Add(view);
            }
            else
            {
                view.Seen(viewedAt);
            }
        }

        // Only the latest ones are kept.
        foreach (var old in views.OrderByDescending(v => v.ViewedAt).Skip(ProductView.MaxPerCustomer))
            _context.Remove(old);
    }
}

// The customer's recently viewed products, newest first.
public class GetRecentlyViewedQuery : IRequest<List<ProductSummaryDto>>
{
    // Leave out the product the customer is looking at now.
    public Guid? Exclude { get; set; }
    public int Take { get; set; } = 12;
}

public class GetRecentlyViewedQueryValidator : AbstractValidator<GetRecentlyViewedQuery>
{
    public GetRecentlyViewedQueryValidator()
    {
        RuleFor(x => x.Take).InclusiveBetween(1, ProductView.MaxPerCustomer);
    }
}

public class GetRecentlyViewedQueryHandler : IRequestHandler<GetRecentlyViewedQuery, List<ProductSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISender _sender;

    public GetRecentlyViewedQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<List<ProductSummaryDto>> Handle(GetRecentlyViewedQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        var ids = await _context.ProductViews
            .Where(v => v.CustomerId == customerId && v.ProductId != request.Exclude)
            .OrderByDescending(v => v.ViewedAt)
            .Select(v => v.ProductId)
            .Take(ProductView.MaxPerCustomer)
            .ToListAsync(cancellationToken);

        var cards = await _sender.InOrderAsync(ids, cancellationToken);
        return cards.Take(request.Take).ToList();
    }
}

// ---------- You may also like ----------

// Products other customers bought in the same orders first, then popular in-stock products
// from the same category and a similar price. If that's not enough: the same category at any
// price, then related categories (same parent), then popular products from the whole shop.
public class GetSimilarProductsQuery : IRequest<List<ProductSummaryDto>>
{
    public Guid ProductId { get; set; }
    public int Take { get; set; } = 8;
}

public class GetSimilarProductsQueryValidator : AbstractValidator<GetSimilarProductsQuery>
{
    public GetSimilarProductsQueryValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Take).InclusiveBetween(1, 20);
    }
}

public class GetSimilarProductsQueryHandler : IRequestHandler<GetSimilarProductsQuery, List<ProductSummaryDto>>
{
    // Similar price: from half to twice the price of the product being looked at.
    private const decimal MinPriceFactor = 0.5m;
    private const decimal MaxPriceFactor = 2m;

    private readonly IApplicationDbContext _context;
    private readonly ISender _sender;

    public GetSimilarProductsQueryHandler(IApplicationDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<List<ProductSummaryDto>> Handle(GetSimilarProductsQuery request, CancellationToken cancellationToken)
    {
        var productId = request.ProductId;

        var product = await _context.Products
            .WherePubliclyVisible(_context)
            .Where(p => p.Id == productId)
            .Select(p => new
            {
                p.CategoryId,
                Price = _context.ProductVariants
                    .Where(v => v.ProductId == p.Id && v.IsActive == true)
                    .Min(v => (decimal?)v.Price) ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Product", productId);

        // A few spare, because some may be out of stock.
        var wanted = request.Take * 2;

        var ids = await BoughtTogetherAsync(productId, wanted, cancellationToken);

        var sameCategory = new List<Guid> { product.CategoryId };
        var related = await RelatedCategoriesAsync(product.CategoryId, cancellationToken);

        // Most similar first; each step only fills what is still missing.
        var steps = new (List<Guid>? Categories, bool SimilarPrice)[]
        {
            (sameCategory, true),
            (sameCategory, false),
            (related, false),
            (null, false)
        };

        foreach (var (categories, similarPrice) in steps)
        {
            if (ids.Count >= wanted)
                break;
            ids.AddRange(await PopularAsync(productId, categories, product.Price, similarPrice, ids, wanted - ids.Count, cancellationToken));
        }

        var cards = await _sender.InOrderAsync(ids, cancellationToken);
        return cards.Where(c => c.InStock).Take(request.Take).ToList();
    }

    private async Task<List<Guid>> BoughtTogetherAsync(Guid productId, int take, CancellationToken cancellationToken)
    {
        var variantIds = _context.ProductVariants.Where(v => v.ProductId == productId).Select(v => v.Id);

        return await _context.Orders
            .Where(o => o.Items.Any(i => variantIds.Contains(i.ProductVariantId)))
            .SelectMany(o => o.Items)
            .Join(_context.ProductVariants, i => i.ProductVariantId, v => v.Id, (i, v) => new { v.ProductId, i.OrderId })
            .Where(x => x.ProductId != productId)
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Orders = g.Select(x => x.OrderId).Distinct().Count() })
            .OrderByDescending(g => g.Orders)
            .ThenBy(g => g.ProductId)
            .Take(take)
            .Select(g => g.ProductId)
            .ToListAsync(cancellationToken);
    }

    // The category's parent, its sister categories and its sub-categories.
    private async Task<List<Guid>> RelatedCategoriesAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var parentId = await _context.Categories
            .Where(c => c.Id == categoryId)
            .Select(c => c.ParentCategoryId)
            .FirstOrDefaultAsync(cancellationToken);

        return await _context.Categories
            .Where(c => c.Id != categoryId
                && (c.ParentCategoryId == categoryId
                    || (parentId != null && (c.Id == parentId || c.ParentCategoryId == parentId))))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    // In-stock checks happen later, on the cards. Null categories means the whole shop.
    private async Task<List<Guid>> PopularAsync(
        Guid productId, List<Guid>? categories, decimal price, bool similarPrice, List<Guid> skip, int take, CancellationToken cancellationToken)
    {
        if (categories is { Count: 0 })
            return new List<Guid>();

        var min = price * MinPriceFactor;
        var max = price * MaxPriceFactor;

        var products = _context.Products
            .WherePubliclyVisible(_context)
            .Where(p => p.Id != productId && !skip.Contains(p.Id));

        if (categories is not null)
            products = products.Where(p => categories.Contains(p.CategoryId));

        var candidates = products
            .Select(p => new
            {
                p.Id,
                p.CreatedAt,
                Price = _context.ProductVariants
                    .Where(v => v.ProductId == p.Id && v.IsActive == true)
                    .Min(v => (decimal?)v.Price) ?? 0,
                // Popularity: how many orders included it.
                Sold = _context.Orders.Count(o => o.Items.Any(i =>
                    _context.ProductVariants.Any(v => v.Id == i.ProductVariantId && v.ProductId == p.Id)))
            });

        if (similarPrice)
            candidates = candidates.Where(c => c.Price >= min && c.Price <= max);

        return await candidates
            .OrderByDescending(c => c.Sold)
            .ThenByDescending(c => c.CreatedAt)
            .Take(take)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
    }
}
