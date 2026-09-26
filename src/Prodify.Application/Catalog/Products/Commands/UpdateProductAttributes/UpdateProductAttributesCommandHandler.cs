using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Products.Commands.UpdateProductAttributes;

public class UpdateProductAttributesCommandHandler : IRequestHandler<UpdateProductAttributesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductAttributesCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateProductAttributesCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Attributes)
            .GetManagedAsync(_currentUser, request.ProductId, cancellationToken);

        product.ReplaceAttributes(request.Attributes.Select(a => (a.Name.Trim(), a.Value.Trim())));
    }
}