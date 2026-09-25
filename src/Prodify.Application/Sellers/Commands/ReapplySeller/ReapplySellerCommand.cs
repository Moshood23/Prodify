using MediatR;

namespace Prodify.Application.Sellers.Commands.ReapplySeller;

// A rejected seller updates their details and sends the application again.
public class ReapplySellerCommand : IRequest
{
    public string BusinessName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Description { get; set; }
}