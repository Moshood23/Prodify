using MediatR;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Products.Commands.SetProductVariantActive;

public class SetProductVariantActiveCommandHandler : IRequestHandler<SetProductVariantActiveCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SetProductVariantActiveCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(SetProductVariantActiveCommand request, CancellationToken cancellationToken)
    {
        var variant = await _context.GetManagedVariantAsync(_currentUser, request.ProductId, request.VariantId, cancellationToken);

        if (request.IsActive)
            variant.Activate();
        else
            variant.Deactivate();
    }
}