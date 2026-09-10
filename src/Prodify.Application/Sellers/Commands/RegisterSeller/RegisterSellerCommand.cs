using MediatR;

namespace Prodify.Application.Sellers.Commands.RegisterSeller;

public class RegisterSellerCommand : IRequest<Guid>
{
    public string BusinessName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}