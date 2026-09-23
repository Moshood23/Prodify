using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Customers.Queries.GetCustomer;

public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, CustomerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCustomerQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CustomerDto> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin() && request.Id != _currentUser.CustomerId)
            throw new NotFoundException("Customer", request.Id);

        var customer = await _context.Customers
            .Where(c => c.Id == request.Id)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                IsActive = c.IsActive,
                Addresses = c.Addresses.Select(a => new CustomerAddressDto
                {
                    Id = a.Id,
                    Label = a.Label,
                    RecipientName = a.RecipientName,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    City = a.City,
                    State = a.State,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    PhoneNumber = a.PhoneNumber,
                    IsDefault = a.IsDefault
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
            throw new NotFoundException("Customer", request.Id);

        return customer;
    }
}