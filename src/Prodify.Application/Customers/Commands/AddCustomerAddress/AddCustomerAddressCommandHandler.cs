using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Customers.Commands.AddCustomerAddress;

public class AddCustomerAddressCommandHandler : IRequestHandler<AddCustomerAddressCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddCustomerAddressCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
     .Include(c => c.Addresses)
     .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

        if (customer is null)
            throw new NotFoundException("Customer", request.CustomerId);

        var address = customer.AddAddress(
            request.Label,
            request.RecipientName,
            request.AddressLine1,
            request.City,
            request.State,
            request.Country,
            request.PhoneNumber,
            request.AddressLine2,
            request.PostalCode,
            request.SetAsDefault);

        await _context.SaveChangesAsync(cancellationToken);

        return address.Id;
    }
}