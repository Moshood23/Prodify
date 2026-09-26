using MediatR;
using Prodify.Application.Common.Interfaces;
using Prodify.Application.Customers.Common;

namespace Prodify.Application.Customers.Commands.UpdateMyProfile;

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateMyProfileCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.GetCurrentCustomerAsync(_currentUser, cancellationToken);

        customer.UpdateProfile(
            request.FirstName,
            request.LastName,
            string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber);
    }
}