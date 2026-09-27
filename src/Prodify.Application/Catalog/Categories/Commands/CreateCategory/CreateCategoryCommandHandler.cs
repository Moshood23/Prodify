using MediatR;
using Prodify.Application.Catalog.Common;
using Prodify.Application.Common.Interfaces;
using Prodify.Domain.Catalog.Entities;

namespace Prodify.Application.Catalog.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        await _context.EnsureValidCategoryAsync(null, request.Name, request.ParentCategoryId, cancellationToken);

        var category = Category.Create(request.Name, request.Description?.Trim(), request.ParentCategoryId);

        _context.Add(category);

        return category.Id;
    }
}