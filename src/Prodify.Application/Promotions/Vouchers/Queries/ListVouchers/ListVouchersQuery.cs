using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Promotions.Vouchers.Common;

namespace Prodify.Application.Promotions.Vouchers.Queries.ListVouchers;

// Every voucher, newest first. There are only ever a handful, so no paging.
public class ListVouchersQuery : IRequest<List<VoucherDto>>
{
}

public class ListVouchersQueryHandler : IRequestHandler<ListVouchersQuery, List<VoucherDto>>
{
    private readonly IApplicationDbContext _context;

    public ListVouchersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<VoucherDto>> Handle(ListVouchersQuery request, CancellationToken cancellationToken) =>
        _context.Vouchers
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VoucherDto
            {
                Id = v.Id,
                Code = v.Code,
                Description = v.Description,
                DiscountType = v.DiscountType.ToString(),
                Value = v.Value,
                MinOrderAmount = v.MinOrderAmount,
                IsActive = v.IsActive,
                TimesUsed = v.TimesUsed,
                CreatedAt = v.CreatedAt
            })
            .ToListAsync(cancellationToken);
}
