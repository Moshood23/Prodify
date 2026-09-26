using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Customers.Common;

namespace Prodify.Application.Customers.Commands.UpdateCustomerAddress;

public class UpdateCustomerAddressCommandHandler : IRequestHandler<UpdateCustomerAddressCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateCustomerAddressCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.GetCurrentCustomerAsync(_currentUser, cancellationToken);
        customer.EnsureHasAddress(request.AddressId);

        customer.UpdateAddress(
            request.AddressId,
            request.Label,
            request.RecipientName,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.PhoneNumber);
    }
}