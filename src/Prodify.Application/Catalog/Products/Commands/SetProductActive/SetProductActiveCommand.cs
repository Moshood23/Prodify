using MediatR;

namespace Prodify.Application.Catalog.Products.Commands.SetProductActive;

// Hides (or shows again) a whole product in the shop.
public class SetProductActiveCommand : IRequest
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}