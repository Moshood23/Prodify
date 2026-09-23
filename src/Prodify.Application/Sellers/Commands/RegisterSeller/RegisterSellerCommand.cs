using MediatR;

namespace Prodify.Application.Sellers.Commands.RegisterSeller;

public class RegisterSellerCommand : IRequest<RegisterSellerResult>
{
    public string BusinessName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
}

public class RegisterSellerResult
{
    public Guid SellerId { get; set; }
    public string Status { get; set; } = null!;

    // A fresh token that includes the new "sellerId" and the Seller role.
    public string Token { get; set; } = null!;
}