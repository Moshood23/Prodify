using MediatR;
using Prodify.Application.Catalog.Products.Common;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateProductCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Sellers can only edit their own products; someone else's product is "not found".
        var product = await _context.Products.GetManagedAsync(_currentUser, request.Id, cancellationToken);

        await _context.EnsureCategoryAndBrandExistAsync(request.CategoryId, request.BrandId, cancellationToken);

        product.Update(request.Name, request.Description, request.CategoryId, request.BrandId);
    }
}