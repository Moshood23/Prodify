using MediatR;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Products.Commands.UpdateProductVariant;

public class UpdateProductVariantCommandHandler : IRequestHandler<UpdateProductVariantCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductVariantCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await _context.GetManagedVariantAsync(_currentUser, request.ProductId, request.VariantId, cancellationToken);

        // Carts show the current price; orders already placed keep the price they were placed at.
        variant.UpdatePrice(request.Price, request.CompareAtPrice);
        variant.UpdateDetails(string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim(), request.Weight);
    }
}