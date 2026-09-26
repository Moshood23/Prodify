using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Customers.Common;

namespace Prodify.Application.Customers.Commands.DeleteCustomerAddress;

// Past orders keep their own copy of the delivery address, so deleting is always safe.
public class DeleteCustomerAddressCommandHandler : IRequestHandler<DeleteCustomerAddressCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteCustomerAddressCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.GetCurrentCustomerAsync(_currentUser, cancellationToken);
        customer.EnsureHasAddress(request.AddressId);

        customer.RemoveAddress(request.AddressId);
    }
}