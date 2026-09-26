using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Customers.Common;

namespace Prodify.Application.Customers.Commands.SetDefaultCustomerAddress;

public class SetDefaultCustomerAddressCommandHandler : IRequestHandler<SetDefaultCustomerAddressCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetDefaultCustomerAddressCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(SetDefaultCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.GetCurrentCustomerAsync(_currentUser, cancellationToken);
        customer.EnsureHasAddress(request.AddressId);

        customer.SetDefaultAddress(request.AddressId);
    }
}