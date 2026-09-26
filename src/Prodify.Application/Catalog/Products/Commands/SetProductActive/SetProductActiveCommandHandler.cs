using MediatR;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Products.Commands.SetProductActive;

public class SetProductActiveCommandHandler : IRequestHandler<SetProductActiveCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetProductActiveCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(SetProductActiveCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.GetManagedAsync(_currentUser, request.Id, cancellationToken);

        if (request.IsActive)
            product.Activate();
        else
            product.Deactivate();
    }
}