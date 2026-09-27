using MediatR;
using Microsoft.EntityFrameworkCore;
using Prodify.Application.Catalog.Common;
using Prodify.Application.Common.Exceptions;
using Prodify.Application.Common.Interfaces;

namespace Prodify.Application.Catalog.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Category", request.Id);

        await _context.EnsureValidCategoryAsync(category.Id, request.Name, request.ParentCategoryId, cancellationToken);

        category.Update(request.Name, string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim());
        category.MoveToParent(request.ParentCategoryId);
    }
}