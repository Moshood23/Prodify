using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Sellers.Queries.GetSeller;

public class GetSellerQueryHandler : IRequestHandler<GetSellerQuery, SellerDto>
{
    private readonly IApplicationDbContext _context;

    public GetSellerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SellerDto> Handle(GetSellerQuery request, CancellationToken cancellationToken)
    {
        var seller = await _context.Sellers
            .Where(s => s.Id == request.Id)
            .Select(s => new SellerDto
            {
                Id = s.Id,
                BusinessName = s.BusinessName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Status = s.Status.ToString(),
                Addresses = s.Addresses.Select(a => new SellerAddressDto
                {
                    Id = a.Id,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    PhoneNumber = a.PhoneNumber
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (seller is null)
            throw new NotFoundException("Seller", request.Id);

        return seller;
    }
}