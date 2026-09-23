using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Common.Security;

namespace Prodify.Application.Customers.Commands.AddCustomerAddress;

public class AddCustomerAddressCommandHandler : IRequestHandler<AddCustomerAddressCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddCustomerAddressCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AddCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentUser.GetRequiredCustomerId();

        var customer = await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            throw new NotFoundException("Customer", customerId);

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

        return address.Id;
    }
}