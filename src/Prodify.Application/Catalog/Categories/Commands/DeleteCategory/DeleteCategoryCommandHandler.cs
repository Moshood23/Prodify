using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Category", request.Id);

        var productCount = await _context.Products.CountAsync(p => p.CategoryId == category.Id, cancellationToken);
        if (productCount > 0)
            throw new BusinessRuleException($"'{category.Name}' has {productCount} product(s). Move them to another category first.");

        if (await _context.Categories.AnyAsync(c => c.ParentCategoryId == category.Id, cancellationToken))
            throw new BusinessRuleException($"'{category.Name}' has sub-categories. Delete or move them first.");

        _context.Remove(category);
    }
}