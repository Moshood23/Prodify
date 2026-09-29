using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Ordering.Entities;
using Prodify.Application.Ordering.Common;

namespace Prodify.Application.Admin.Customers.Queries.GetCustomerDetails;

public class GetCustomerDetailsQueryHandler : IRequestHandler<GetCustomerDetailsQuery, CustomerDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerDetailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDetailsDto> Handle(GetCustomerDetailsQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Customer", request.Id);

        var orders = _context.Orders.Where(o => o.CustomerId == customer.Id);

        return new CustomerDetailsDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            OrderCount = await orders.CountAsync(cancellationToken),
            CancelledOrderCount = await orders.CountAsync(o => o.SellerOrders.All(so => so.Status == SellerOrderStatus.Cancelled), cancellationToken),
            TotalSpent = await _context.SoldItems(orders.Where(o => o.IsPaid))
                .SumAsync(i => (decimal?)(i.UnitPrice.Amount * i.Quantity), cancellationToken) ?? 0,
            Addresses = customer.Addresses
                .OrderByDescending(a => a.IsDefault)
                .Select(a => new CustomerAddressSummaryDto
                {
                    Label = a.Label,
                    RecipientName = a.RecipientName,
                    AddressLine1 = a.AddressLine1,
                    City = a.City,
                    State = a.State,
                    PhoneNumber = a.PhoneNumber,
                    IsDefault = a.IsDefault
                })
                .ToList()
        };
    }
}