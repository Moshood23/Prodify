using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Customers.StoreCredit.Queries.GetMyStoreCredit;

public class GetMyStoreCreditQueryHandler : IRequestHandler<GetMyStoreCreditQuery, StoreCreditDto>
{
    private const int LatestEntries = 50;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyStoreCreditQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<StoreCreditDto> Handle(GetMyStoreCreditQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        var entries = await _context.StoreCreditEntries
            .Where(e => e.CustomerId == customerId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(LatestEntries)
            .Select(e => new StoreCreditEntryDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Kind = e.Kind.ToString(),
                Description = e.Description,
                OrderId = e.OrderId,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new StoreCreditDto
        {
            Balance = await _context.GetStoreCreditBalanceAsync(customerId, cancellationToken),
            Entries = entries
        };
    }
}
