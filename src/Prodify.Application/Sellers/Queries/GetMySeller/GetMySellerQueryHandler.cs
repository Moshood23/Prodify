using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;
using Prodify.Application.Sellers.Common;

namespace Prodify.Application.Sellers.Queries.GetMySeller;

public class GetMySellerQueryHandler : IRequestHandler<GetMySellerQuery, SellerProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMySellerQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SellerProfileDto> Handle(GetMySellerQuery request, CancellationToken cancellationToken)
    {
        var sellerId = _currentUser.GetRequiredSellerId();

        return await _context.Sellers
            .Where(s => s.Id == sellerId)
            .Select(SellerProjections.ToProfile)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Seller", sellerId);
    }
}